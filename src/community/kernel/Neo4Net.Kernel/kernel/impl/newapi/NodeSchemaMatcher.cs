using System;
using System.Collections.Generic;
using System.Diagnostics;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.Impl.Newapi
{

	using Neo4Net.Functions;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptor;
	using SchemaDescriptorSupplier = Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptorSupplier;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.collection.PrimitiveArrays.isSortedSet;

	/// <summary>
	/// This class holds functionality to match LabelSchemaDescriptors to nodes
	/// </summary>
	public class NodeSchemaMatcher
	{
		 private NodeSchemaMatcher()
		 {
			  throw new AssertionError( "no instance" );
		 }

		 /// <summary>
		 /// Iterate over some schema suppliers, and invoke a callback for every supplier that matches the node. To match the
		 /// node N the supplier must supply a LabelSchemaDescriptor D, such that N has values for all the properties of D.
		 /// The supplied schemas are all assumed to match N on label.
		 /// <para>
		 /// To avoid unnecessary store lookups, this implementation only gets propertyKeyIds for the node if some
		 /// descriptor has a valid label.
		 /// 
		 /// </para>
		 /// </summary>
		 /// @param <SUPPLIER> the type to match. Must implement SchemaDescriptorSupplier </param>
		 /// @param <EXCEPTION> The type of exception that can be thrown when taking the action </param>
		 /// <param name="schemaSuppliers"> The suppliers to match </param>
		 /// <param name="specialPropertyId"> This property id will always count as a match for the descriptor, regardless of
		 /// whether the node has this property or not </param>
		 /// <param name="existingPropertyIds"> sorted array of property ids for the IEntity to match schema for. </param>
		 /// <param name="callback"> The action to take on match </param>
		 /// <exception cref="EXCEPTION"> This exception is propagated from the action </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static <SUPPLIER extends org.Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptorSupplier, EXCEPTION extends Exception> void onMatchingSchema(java.util.Iterator<SUPPLIER> schemaSuppliers, int specialPropertyId, int[] existingPropertyIds, org.Neo4Net.function.ThrowingConsumer<SUPPLIER,EXCEPTION> callback) throws EXCEPTION
		 internal static void OnMatchingSchema<SUPPLIER, EXCEPTION>( IEnumerator<SUPPLIER> schemaSuppliers, int specialPropertyId, int[] existingPropertyIds, ThrowingConsumer<SUPPLIER, EXCEPTION> callback ) where SUPPLIER : Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptorSupplier where EXCEPTION : Exception
		 {
			  Debug.Assert( isSortedSet( existingPropertyIds ) );
			  while ( schemaSuppliers.MoveNext() )
			  {
					SUPPLIER schemaSupplier = schemaSuppliers.Current;
					SchemaDescriptor schema = schemaSupplier.schema();

					if ( NodeHasSchemaProperties( existingPropertyIds, Schema.PropertyIds, specialPropertyId ) )
					{
						 callback.Accept( schemaSupplier );
					}
			  }
		 }

		 /// <summary>
		 /// Iterate over some schema suppliers, and invoke a callback for every supplier that matches the node. To match the
		 /// node N the supplier must supply a LabelSchemaDescriptor D, such that N has the label of D, and values for all
		 /// the properties of D.
		 /// <para>
		 /// To avoid unnecessary store lookups, this implementation only gets propertyKeyIds for the node if some
		 /// descriptor has a valid label.
		 /// 
		 /// </para>
		 /// </summary>
		 /// @param <SUPPLIER> the type to match. Must implement SchemaDescriptorSupplier </param>
		 /// @param <EXCEPTION> The type of exception that can be thrown when taking the action </param>
		 /// <param name="schemaSuppliers"> The suppliers to match </param>
		 /// <param name="specialPropertyId"> This property id will always count as a match for the descriptor, regardless of
		 /// whether the node has this property or not </param>
		 /// <param name="existingPropertyIds"> sorted array of property ids for the IEntity to match schema for. </param>
		 /// <param name="callback"> The action to take on match </param>
		 /// <exception cref="EXCEPTION"> This exception is propagated from the action </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static <SUPPLIER extends org.Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptorSupplier, EXCEPTION extends Exception> void onMatchingSchema(java.util.Iterator<SUPPLIER> schemaSuppliers, long[] labels, int specialPropertyId, int[] existingPropertyIds, org.Neo4Net.function.ThrowingConsumer<SUPPLIER,EXCEPTION> callback) throws EXCEPTION
		 internal static void OnMatchingSchema<SUPPLIER, EXCEPTION>( IEnumerator<SUPPLIER> schemaSuppliers, long[] labels, int specialPropertyId, int[] existingPropertyIds, ThrowingConsumer<SUPPLIER, EXCEPTION> callback ) where SUPPLIER : Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptorSupplier where EXCEPTION : Exception
		 {
			  Debug.Assert( isSortedSet( existingPropertyIds ) );
			  Debug.Assert( isSortedSet( labels ) );
			  while ( schemaSuppliers.MoveNext() )
			  {
					SUPPLIER schemaSupplier = schemaSuppliers.Current;
					SchemaDescriptor schema = schemaSupplier.schema();

					if ( !Schema.isAffected( labels ) )
					{
						 continue;
					}

					if ( NodeHasSchemaProperties( existingPropertyIds, Schema.PropertyIds, specialPropertyId ) )
					{
						 callback.Accept( schemaSupplier );
					}
			  }
		 }

		 private static bool NodeHasSchemaProperties( int[] existingPropertyIds, int[] indexPropertyIds, int changedPropertyId )
		 {
			  foreach ( int indexPropertyId in indexPropertyIds )
			  {
					if ( indexPropertyId != changedPropertyId && !Contains( existingPropertyIds, indexPropertyId ) )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 private static bool Contains( int[] existingPropertyIds, int indexPropertyId )
		 {
			  return Arrays.binarySearch( existingPropertyIds, indexPropertyId ) >= 0;
		 }
	}

}