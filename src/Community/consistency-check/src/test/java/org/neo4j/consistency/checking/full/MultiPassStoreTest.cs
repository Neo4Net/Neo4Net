using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Consistency.checking.full
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using JUnit4 = org.junit.runners.JUnit4;
	using Suite = org.junit.runners.Suite;

	using RecordAccess = Neo4Net.Consistency.store.RecordAccess;
	using Neo4Net.Consistency.store;
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;
	using DynamicRecord = Neo4Net.Kernel.impl.store.record.DynamicRecord;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using PropertyRecord = Neo4Net.Kernel.impl.store.record.PropertyRecord;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Suite.class) @Suite.SuiteClasses({ MultiPassStoreTest.Nodes.class, MultiPassStoreTest.Relationships.class, MultiPassStoreTest.Properties.class, MultiPassStoreTest.Strings.class, MultiPassStoreTest.Arrays.class }) public abstract class MultiPassStoreTest
	public abstract class MultiPassStoreTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSkipOtherKindsOfRecords()
		 public virtual void ShouldSkipOtherKindsOfRecords()
		 {
			  // given
			  RecordAccess recordAccess = mock( typeof( RecordAccess ) );

			  // when
			  IList<RecordAccess> filters = MultiPassStore().multiPassFilters(recordAccess, MultiPassStore.values());

			  // then
			  foreach ( RecordAccess filter in filters )
			  {
					foreach ( long id in new long[] { 0, 100, 200, 300, 400, 500, 600, 700, 800, 900 } )
					{
						 OtherRecords( filter, id );
					}
			  }

			  verifyZeroInteractions( recordAccess );
		 }

		 protected internal abstract MultiPassStore MultiPassStore();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: protected abstract org.neo4j.consistency.store.RecordReference<? extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord> record(org.neo4j.consistency.store.RecordAccess filter, long id);
		 protected internal abstract RecordReference<AbstractBaseRecord> Record( RecordAccess filter, long id );

		 protected internal abstract void OtherRecords( RecordAccess filter, long id );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(JUnit4.class) public static class Nodes extends MultiPassStoreTest
		 public class Nodes : MultiPassStoreTest
		 {
			  protected internal override MultiPassStore MultiPassStore()
			  {
					return MultiPassStore.Nodes;
			  }

			  protected internal override RecordReference<NodeRecord> Record( RecordAccess filter, long id )
			  {
					return filter.Node( id );
			  }

			  protected internal override void OtherRecords( RecordAccess filter, long id )
			  {
					filter.Relationship( id );
					filter.Property( id );
					filter.String( id );
					filter.Array( id );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(JUnit4.class) public static class Relationships extends MultiPassStoreTest
		 public class Relationships : MultiPassStoreTest
		 {
			  protected internal override MultiPassStore MultiPassStore()
			  {
					return MultiPassStore.Relationships;
			  }

			  protected internal override RecordReference<RelationshipRecord> Record( RecordAccess filter, long id )
			  {
					return filter.Relationship( id );
			  }

			  protected internal override void OtherRecords( RecordAccess filter, long id )
			  {
					filter.Node( id );
					filter.Property( id );
					filter.String( id );
					filter.Array( id );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(JUnit4.class) public static class Properties extends MultiPassStoreTest
		 public class Properties : MultiPassStoreTest
		 {
			  protected internal override MultiPassStore MultiPassStore()
			  {
					return MultiPassStore.Properties;
			  }

			  protected internal override RecordReference<PropertyRecord> Record( RecordAccess filter, long id )
			  {
					return filter.Property( id );
			  }

			  protected internal override void OtherRecords( RecordAccess filter, long id )
			  {
					filter.Node( id );
					filter.Relationship( id );
					filter.String( id );
					filter.Array( id );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(JUnit4.class) public static class Strings extends MultiPassStoreTest
		 public class Strings : MultiPassStoreTest
		 {
			  protected internal override MultiPassStore MultiPassStore()
			  {
					return MultiPassStore.Strings;
			  }

			  protected internal override RecordReference<DynamicRecord> Record( RecordAccess filter, long id )
			  {
					return filter.String( id );
			  }

			  protected internal override void OtherRecords( RecordAccess filter, long id )
			  {
					filter.Node( id );
					filter.Relationship( id );
					filter.Property( id );
					filter.Array( id );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(JUnit4.class) public static class Arrays extends MultiPassStoreTest
		 public class Arrays : MultiPassStoreTest
		 {
			  protected internal override MultiPassStore MultiPassStore()
			  {
					return MultiPassStore.Arrays;
			  }

			  protected internal override RecordReference<DynamicRecord> Record( RecordAccess filter, long id )
			  {
					return filter.Array( id );
			  }

			  protected internal override void OtherRecords( RecordAccess filter, long id )
			  {
					filter.Node( id );
					filter.Relationship( id );
					filter.Property( id );
					filter.String( id );
			  }
		 }
	}

}