//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Collections.Generic;
using Dse.Data.Linq;
using NUnit.Framework;
#pragma warning disable 618

namespace Dse.Test.Integration.CqlFunctions.Structures
{
    [Table("EntityWithTimeUuid")]
    [AllowFiltering]
    public class EntityWithTimeUuid
    {
        private const int DefaultRecordCount = 6;

        [PartitionKey]
        [Column("string_type")]
        public string StringType = "someStringVal";

        [Column("guid_type")]
        public Guid GuidType { get; set; }

        [Column("time_uuid_type")]
        [ClusteringKey(1)]
        public TimeUuid TimeUuidType { get; set; }

        public static void AssertEquals(EntityWithTimeUuid expectedEntity, EntityWithTimeUuid actualEntity)
        {
            Assert.AreEqual(expectedEntity.GuidType, actualEntity.GuidType);
            Assert.AreEqual(expectedEntity.StringType, actualEntity.StringType);
            Assert.AreEqual(expectedEntity.TimeUuidType, actualEntity.TimeUuidType);
        }

        public static bool AssertListContains(List<EntityWithTimeUuid> expectedEntities, EntityWithTimeUuid actualEntity)
        {
            foreach (var expectedEntity in expectedEntities)
            {
                try
                {
                    AssertEquals(expectedEntity, actualEntity);
                    return true;
                }
                catch (AssertionException) { }
            }
            return false;
        }

        public static List<EntityWithTimeUuid> GetDefaultObjectList()
        {
            List<EntityWithTimeUuid> defaultTimeUuidObjList = new List<EntityWithTimeUuid>();
            for (int i = 1; i <= DefaultRecordCount; i++)
            {
                EntityWithTimeUuid entity = new EntityWithTimeUuid();
                entity.TimeUuidType = TimeUuid.NewId(DateTimeOffset.Parse("2014-3-" + i));
                entity.GuidType = Guid.NewGuid();
                defaultTimeUuidObjList.Add(entity);
            }
            return defaultTimeUuidObjList;
        }

        public static void SetupEntity(Table<EntityWithTimeUuid> tableEntityWithTimeUuid, List<EntityWithTimeUuid> expectedTimeUuidObjectList)
        {
            //Insert some data
            foreach (var expectedObj in expectedTimeUuidObjectList)
                tableEntityWithTimeUuid.Insert(expectedObj).Execute();
        }
    }
}
