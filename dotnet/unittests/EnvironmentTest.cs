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

using Hamster;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Unittests
{
    public class EnvironmentTest
    {
        void checkEqual(byte[] lhs, byte[] rhs)
        {
            Assert.AreEqual(lhs.Length, rhs.Length);
            for (int i = 0; i < lhs.Length; i++)
                Assert.AreEqual(lhs[i], rhs[i]);
        }

        private void CreateString() {
            Hamster.Environment env = new Hamster.Environment();
            try {
                env.Create("ntest.db");
                env.Close();
            }
            catch (DatabaseException e) {
                Assert.Fail("unexpected exception " + e);
            }
        }

        private void CreateStringNull()
        {
            Hamster.Environment env = new Hamster.Environment();
            try {
                env.Create(null);
                env.Close();
            }
            catch (DatabaseException e) {
                Assert.AreEqual(HamConst.HAM_INV_PARAMETER, e.ErrorCode);
            }
        }

        private void CreateStringInt() {
            Hamster.Environment env = new Hamster.Environment();
            try {
                env.Create(null, HamConst.HAM_IN_MEMORY);
                env.Close();
            }
            catch (DatabaseException e) {
                Assert.Fail("unexpected exception " + e);
            }
        }

        private void CreateStringIntInt() {
            Hamster.Environment env = new Hamster.Environment();
            try {
                env.Create("ntest.db", 0, 0644);
                env.Close();
            }
            catch (DatabaseException e) {
                Assert.Fail("unexpected exception " + e);
            }
        }

        private void CreateStringIntIntParameter()
        {
            Hamster.Environment env = new Hamster.Environment();
            Parameter[] param = new Parameter[1];
            param[0] = new Parameter();
            param[0].name = HamConst.HAM_PARAM_CACHE_SIZE;
            param[0].value = 1000;
            try {
                env.Create("ntest.db", 0, 0644, param);
                env.Close();
            }
            catch (DatabaseException e) {
                Assert.Fail("Unexpected exception " + e);
            }
        }

        private void CreateStringIntIntParameterNeg() {
            Hamster.Environment env = new Hamster.Environment();
            Parameter[] param = new Parameter[1];
            param[0] = new Parameter();
            param[0].name = HamConst.HAM_PARAM_PAGESIZE;
            param[0].value = 777;
            try {
                env.Create("ntest.db", 0, 0644, param);
                env.Close();
            }
            catch (DatabaseException e) {
                Assert.AreEqual(HamConst.HAM_INV_PAGESIZE, e.ErrorCode);
            }
        }

        private void OpenString() {
            Hamster.Environment env = new Hamster.Environment();
            try {
                env.Create("ntest.db");
                env.Close();
                env.Open("ntest.db");
                env.Close();
            }
            catch (DatabaseException e) {
                Assert.Fail("unexpected exception " + e);
            }
        }

        private void OpenStringNegative() {
            Hamster.Environment env = new Hamster.Environment();
            try {
                env.Open("ntestxxxxx.db");
                env.Close();
            }
            catch (DatabaseException e) {
                Assert.AreEqual(HamConst.HAM_FILE_NOT_FOUND, e.ErrorCode);
            }
        }

        private void OpenStringIntIntParameter() {
            Hamster.Environment env = new Hamster.Environment();
            Parameter[] param = new Parameter[1];
            param[0] = new Parameter();
            param[0].name = HamConst.HAM_PARAM_CACHESIZE;
            param[0].value = 1024;
            try {
                env.Create("ntest.db", 0, 0644, param);
                env.Close();
                env.Open("ntest.db", 0, param);
                env.Close();
            }
            catch (DatabaseException e) {
                Assert.Fail("unexpected exception " + e);
            }
        }

        private void CreateDatabaseShort() {
            Hamster.Environment env = new Hamster.Environment();
            byte[] k=new byte[5];
            byte[] r=new byte[5];
            try {
                env.Create("ntest.db");
                Database db = env.CreateDatabase((short)13);
                db.Insert(k, r);
                db.Close();
                db = env.OpenDatabase((short)13);
                byte[] f = db.Find(k);
                checkEqual(r, f);
                // db.Close();
                env.Close();
            }
            catch (DatabaseException e) {
                Assert.Fail("unexpected exception " + e);
            }
        }

        private void CreateDatabaseNegative() {
            Hamster.Environment env = new Hamster.Environment();
            try {
                env.Create("ntest.db");
                Database db = env.CreateDatabase((short)0);
            }
            catch (DatabaseException e) {
                Assert.AreEqual(HamConst.HAM_INV_PARAMETER, e.ErrorCode);
            }
            env.Close();
        }

        private void OpenDatabaseNegative() {
            Hamster.Environment env = new Hamster.Environment();
            try {
                env.Create("ntest.db");
                Database db = env.OpenDatabase((short)99);
            }
            catch (DatabaseException e) {
                Assert.AreEqual(HamConst.HAM_DATABASE_NOT_FOUND, e.ErrorCode);
            }
            env.Close();
        }

        private void RenameDatabase() {
            Hamster.Environment env = new Hamster.Environment();
            byte[] k = new byte[5];
            byte[] r = new byte[5];
            try {
                env.Create("ntest.db");
                Database db = env.CreateDatabase((short)13);
                db.Insert(k, r);
                db.Close();
                env.RenameDatabase((short)13, (short)15);
                db = env.OpenDatabase((short)15);
                byte[] f = db.Find(k);
                checkEqual(r, f);
                // db.Close();
                env.Close();
            }
            catch (DatabaseException e) {
                Assert.Fail("unexpected exception " + e);
            }
        }

        private void EraseDatabase() {
            Hamster.Environment env = new Hamster.Environment();
            byte[] k = new byte[5];
            byte[] r = new byte[5];

            env.Create("ntest.db");
            Database db = env.CreateDatabase((short)13);
            db.Insert(k, r);
            db.Close();
            env.EraseDatabase((short)13);
            try {
                db = env.OpenDatabase((short)15);
            }
            catch (DatabaseException e) {
                Assert.AreEqual(HamConst.HAM_DATABASE_NOT_FOUND, e.ErrorCode);
            }
            env.Close();
        }

        private void EraseUnknownDatabase() {
            Hamster.Environment env = new Hamster.Environment();
            byte[] k = new byte[5];
            byte[] r = new byte[5];

            env.Create("ntest.db");
            Database db = env.CreateDatabase((short)13);
            db.Insert(k, r);
            db.Close();
            try {
                env.EraseDatabase((short)99);
            }
            catch (DatabaseException e) {
                Assert.AreEqual(HamConst.HAM_DATABASE_NOT_FOUND, e.ErrorCode);
            }
            env.Close();
        }

        private void Flush()
        {
            Hamster.Environment env = new Hamster.Environment();
            env.Create("ntest.db");
            env.Flush();
            env.Close();
        }

        private void GetDatabaseNames() {
            Database db;
            short[] names;
            short[] s1 ={ 13 };
            short[] s2 ={ 13, 14 };
            short[] s3 ={ 13, 14, 15 };
            Hamster.Environment env = new Hamster.Environment();

            env.Create("ntest.db");
            db = env.CreateDatabase(13);
            names = env.GetDatabaseNames();
            Assert.AreEqual(s1.Length, names.Length);
            for (int i = 0; i < s1.Length; i++)
                Assert.AreEqual(s1[i], names[i]);

            db = env.CreateDatabase(14);
            names = env.GetDatabaseNames();
            Assert.AreEqual(s2.Length, names.Length);
            for (int i = 0; i < s2.Length; i++)
                Assert.AreEqual(s2[i], names[i]);

            db = env.CreateDatabase(15);
            names = env.GetDatabaseNames();
            Assert.AreEqual(s3.Length, names.Length);
            for (int i = 0; i < s3.Length; i++)
                Assert.AreEqual(s3[i], names[i]);

            env.Close();
        }

        public void Run()
        {
            Console.WriteLine("EnvironmentTest.CreateDatabaseNegative");
            CreateDatabaseNegative();

            Console.WriteLine("EnvironmentTest.CreateDatabaseShort");
            CreateDatabaseShort();

            Console.WriteLine("EnvironmentTest.CreateString");
            CreateString();

            Console.WriteLine("EnvironmentTest.CreateStringInt");
            CreateStringInt();

            Console.WriteLine("EnvironmentTest.CreateStringIntInt");
            CreateStringIntInt();

            Console.WriteLine("EnvironmentTest.CreateStringIntIntParameter");
            CreateStringIntIntParameter();

            Console.WriteLine("EnvironmentTest.CreateStringIntIntParameterNeg");
            CreateStringIntIntParameterNeg();

            Console.WriteLine("EnvironmentTest.CreateStringNull");
            CreateStringNull();

            Console.WriteLine("EnvironmentTest.RenameDatabase"); 
            RenameDatabase();

            Console.WriteLine("EnvironmentTest.EraseDatabase"); 
            EraseDatabase();

            Console.WriteLine("EnvironmentTest.EraseUnknownDatabase");
            EraseUnknownDatabase();

            Console.WriteLine("EnvironmentTest.OpenDatabaseNegative");
            OpenDatabaseNegative();

            Console.WriteLine("EnvironmentTest.Flush");
            Flush();

            Console.WriteLine("EnvironmentTest.GetDatabaseNames");
            GetDatabaseNames();
        }
    }
}
