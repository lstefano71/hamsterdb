
AM_CPPFLAGS     = -I../include -I$(top_builddir)/include

noinst_PROGRAMS = db1 db2 db3 db4 db5 db6 env1 env2 env3

noinst_BIN      = db1 db2 db3 db4 db5 db6 env1 env2 env3

if ENABLE_REMOTE
noinst_PROGRAMS += server1 client1
noinst_BIN      += server1 client1
endif

db1_SOURCES     = db1.c
db1_LDADD       = $(top_builddir)/src/libhamsterdb.la

db2_SOURCES     = db2.c
db2_LDADD       = $(top_builddir)/src/libhamsterdb.la

db3_SOURCES     = db3.c
db3_LDADD       = $(top_builddir)/src/libhamsterdb.la

db4_SOURCES     = db4.c
db4_LDADD       = $(top_builddir)/src/libhamsterdb.la

db5_SOURCES     = db5.c
db5_LDADD       = $(top_builddir)/src/libhamsterdb.la

db6_SOURCES     = db6.cpp
db6_LDADD       = $(top_builddir)/src/libhamsterdb.la

env1_SOURCES    = env1.c
env1_LDADD      = $(top_builddir)/src/libhamsterdb.la

env2_SOURCES    = env2.c
env2_LDADD      = $(top_builddir)/src/libhamsterdb.la

env3_SOURCES    = env3.cpp
env3_LDADD      = $(top_builddir)/src/libhamsterdb.la

server1_SOURCES = server1.c
server1_LDADD   = $(top_builddir)/src/libhamsterdb.la \
                  $(top_builddir)/src/5server/libhamserver.la -ldl -lpthread

client1_SOURCES = client1.c
client1_LDADD   = $(top_builddir)/src/libhamsterdb.la \
                  $(top_builddir)/src/5server/libhamserver.la -ldl -lpthread

