﻿/*
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
namespace Org.Neo4j.Kernel.impl.store.record
{

	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using IndexDescriptorFactory = Org.Neo4j.Storageengine.Api.schema.IndexDescriptorFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;

	internal abstract class SchemaRuleTestBase
	{
		 protected internal const long RULE_ID = 1;
		 protected internal const long RULE_ID_2 = 2;
		 protected internal const int LABEL_ID = 10;
		 protected internal const int LABEL_ID_2 = 11;
		 protected internal const int REL_TYPE_ID = 20;
		 protected internal const int PROPERTY_ID_1 = 30;
		 protected internal const int PROPERTY_ID_2 = 31;

		 protected internal static readonly IndexProviderDescriptor ProviderDescriptor = new IndexProviderDescriptor( "index-provider", "1.0" );

		 protected internal virtual void AssertEquality( object o1, object o2 )
		 {
			  assertThat( o1, equalTo( o2 ) );
			  assertThat( o2, equalTo( o1 ) );
			  assertThat( o1.GetHashCode(), equalTo(o2.GetHashCode()) );
		 }

		 public static IndexDescriptor ForLabel( int labelId, params int[] propertyIds )
		 {
			  return IndexDescriptorFactory.forSchema( SchemaDescriptorFactory.forLabel( labelId, propertyIds ), ProviderDescriptor );
		 }

		 public static IndexDescriptor NamedForLabel( string name, int labelId, params int[] propertyIds )
		 {
			  return IndexDescriptorFactory.forSchema( SchemaDescriptorFactory.forLabel( labelId, propertyIds ), name, ProviderDescriptor );
		 }

		 public static IndexDescriptor UniqueForLabel( int labelId, params int[] propertyIds )
		 {
			  return IndexDescriptorFactory.uniqueForSchema( SchemaDescriptorFactory.forLabel( labelId, propertyIds ), null, ProviderDescriptor );
		 }

		 public static IndexDescriptor NamedUniqueForLabel( string name, int labelId, params int[] propertyIds )
		 {
			  return IndexDescriptorFactory.uniqueForSchema( SchemaDescriptorFactory.forLabel( labelId, propertyIds ), name, ProviderDescriptor );
		 }
	}

}