/*
 * Copyright (C) 2005-2015 Christoph Rupp (chris@crupp.de).
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/**
 * @brief functions for reading/writing/allocating blobs (memory chunks of
 * arbitrary size)
 *
 */

#ifndef HAM_BLOB_MANAGER_H
#define HAM_BLOB_MANAGER_H

#include "0root/root.h"

#include "ham/hamsterdb_int.h"

// Always verify that a file of level N does not include headers > N!
#include "1base/dynamic_array.h"
#include "2page/page.h"

#ifndef HAM_ROOT_H
#  error "root.h was not included"
#endif

namespace hamsterdb {

class Context;
class Device;
class PageManager;
class EnvironmentConfiguration;

#include "1base/packstart.h"

// A blob header structure
//
// This header is prepended to the blob's payload. It holds the blob size and
// the blob's address (which is not required but useful for error checking.)
HAM_PACK_0 struct HAM_PACK_1 PBlobHeader
{
  PBlobHeader() {
    ::memset(this, 0, sizeof(PBlobHeader));
  }

  // Returns a PBlobHeader from a file address
  static PBlobHeader *from_page(Page *page, uint64_t address) {
    uint32_t readstart = (uint32_t)(address - page->get_address());
    return (PBlobHeader *)&page->get_raw_payload()[readstart];
  }

  // Flags; currently only used in hamsterdb-pro to store compression
  // information
  uint32_t flags;

  // The blob ID - which is the absolute address/offset of this
  // structure in the file
  uint64_t blob_id;

  // The allocated size of the blob; this is the size, which is used
  // by the blob and it's header and maybe additional padding
  uint64_t allocated_size;

  // The size of the blob from the user's point of view (excluding the header)
  uint64_t size;

} HAM_PACK_2;

#include "1base/packstop.h"

// The BlobManager manages blobs (not a surprise)
//
// This is an abstract baseclass, derived for In-Memory- and Disk-based
// Environments.
class BlobManager
{
  protected:
    // Flags for the PBlobHeader structure
    enum {
      // Blob is compressed
      kIsCompressed = 1
    };

  public:
    // Flags for allocate(); make sure that they do not conflict with
    // the flags for ham_db_insert()
    enum {
      // Do not compress the blob, even if compression is enabled
      kDisableCompression = 0x10000000
    };

    BlobManager(const EnvironmentConfiguration *config,
                    PageManager *page_manager, Device *device)
      : m_config(config), m_page_manager(page_manager), m_device(device),
        m_metric_before_compression(0),
        m_metric_after_compression(0), m_metric_total_allocated(0),
        m_metric_total_read(0) {
    }

    virtual ~BlobManager() { }

    // Allocates/create a new blob.
    // This function returns the blob-id (the start address of the blob
    // header)
    //
    // |flags| can be HAM_PARTIAL, kDisableCompression
    uint64_t allocate(Context *context, ham_record_t *record, uint32_t flags);

    // Reads a blob and stores the data in @a record.
    // @ref flags: either 0 or HAM_DIRECT_ACCESS
    void read(Context *context, uint64_t blob_id, ham_record_t *record,
                    uint32_t flags, ByteArray *arena);

    // Retrieves the size of a blob
    uint64_t get_blob_size(Context *context, uint64_t blob_id);

    // Overwrites an existing blob
    //
    // Will return an error if the blob does not exist. Returns the blob-id
    // (the start address of the blob header)
    uint64_t overwrite(Context *context, uint64_t old_blob_id,
                    ham_record_t *record, uint32_t flags);

    // Deletes an existing blob
    void erase(Context *context, uint64_t blob_id, Page *page = 0,
                    uint32_t flags = 0);

    // Fills in the current metrics
    void fill_metrics(ham_env_metrics_t *metrics) const {
      metrics->blob_total_allocated = m_metric_total_allocated;
      metrics->blob_total_read = m_metric_total_read;
      metrics->record_bytes_before_compression = m_metric_before_compression;
      metrics->record_bytes_after_compression = m_metric_after_compression;
    }

  protected:
    // Allocates/create a new blob.
    // This function returns the blob-id (the start address of the blob
    // header)
    virtual uint64_t do_allocate(Context *context, ham_record_t *record,
                    uint32_t flags) = 0;

    // Reads a blob and stores the data in @a record.
    // @ref flags: either 0 or HAM_DIRECT_ACCESS
    virtual void do_read(Context *context, uint64_t blob_id,
                    ham_record_t *record, uint32_t flags,
                    ByteArray *arena) = 0;

    // Retrieves the size of a blob
    virtual uint64_t do_get_blob_size(Context *context,
                    uint64_t blob_id) = 0;

    // Overwrites an existing blob
    //
    // Will return an error if the blob does not exist. Returns the blob-id
    // (the start address of the blob header)
    virtual uint64_t do_overwrite(Context *context, uint64_t old_blob_id,
                    ham_record_t *record, uint32_t flags) = 0;

    // Deletes an existing blob
    virtual void do_erase(Context *context, uint64_t blob_id,
                    Page *page = 0, uint32_t flags = 0) = 0;

    // The configuration of the Environment
    const EnvironmentConfiguration *m_config;

    // The active page manager - required to allocate and fetch pages
    PageManager *m_page_manager;

    // The device - sometimes it's accessed directly
    Device *m_device;

    // Usage tracking - number of bytes before compression
    uint64_t m_metric_before_compression;

    // Usage tracking - number of bytes after compression
    uint64_t m_metric_after_compression;

  private:
    // Usage tracking - number of blobs allocated
    uint64_t m_metric_total_allocated;

    // Usage tracking - number of blobs read
    uint64_t m_metric_total_read;
};

} // namespace hamsterdb

#endif /* HAM_BLOB_MANAGER_H */
