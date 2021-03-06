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
namespace Org.Neo4j.Storageengine.Api.schema
{

	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using SchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.schema.IndexProviderDescriptor.UNDECIDED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.schema.IndexDescriptor.Type.GENERAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.schema.IndexDescriptor.Type.UNIQUE;

	public class IndexDescriptorFactory
	{
		 private IndexDescriptorFactory()
		 {
		 }

		 public static IndexDescriptor ForSchema( SchemaDescriptor schema )
		 {
			  return ForSchema( schema, UNDECIDED );
		 }

		 public static IndexDescriptor ForSchema( SchemaDescriptor schema, IndexProviderDescriptor providerDescriptor )
		 {
			  return ForSchema( schema, null, providerDescriptor );
		 }

		 public static IndexDescriptor ForSchema( SchemaDescriptor schema, Optional<string> name, IndexProviderDescriptor providerDescriptor )
		 {
			  return new IndexDescriptor( schema, GENERAL, name, providerDescriptor );
		 }

		 public static IndexDescriptor UniqueForSchema( SchemaDescriptor schema )
		 {
			  return UniqueForSchema( schema, UNDECIDED );
		 }

		 public static IndexDescriptor UniqueForSchema( SchemaDescriptor schema, IndexProviderDescriptor providerDescriptor )
		 {
			  return UniqueForSchema( schema,null, providerDescriptor );
		 }

		 public static IndexDescriptor UniqueForSchema( SchemaDescriptor schema, Optional<string> name, IndexProviderDescriptor providerDescriptor )
		 {
			  return new IndexDescriptor( schema, UNIQUE, name, providerDescriptor );
		 }
	}

}