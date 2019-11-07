using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.coreapi.schema
{

	using IndexDefinition = Neo4Net.GraphDb.Schema.IndexDefinition;
	using TokenNameLookup = Neo4Net.Kernel.Api.Internal.TokenNameLookup;
	using TokenRead = Neo4Net.Kernel.Api.Internal.TokenRead;
	using TokenWrite = Neo4Net.Kernel.Api.Internal.TokenWrite;
	using PropertyKeyIdNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.PropertyKeyIdNotFoundKernelException;
	using IllegalTokenNameException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IllegalTokenNameException;
	using LabelSchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.LabelSchemaDescriptor;

	public class PropertyNameUtils
	{
		 private PropertyNameUtils()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static String[] getPropertyKeys(Neo4Net.Kernel.Api.Internal.TokenRead tokenRead, int...properties) throws Neo4Net.Kernel.Api.Internal.Exceptions.PropertyKeyIdNotFoundKernelException
		 public static string[] GetPropertyKeys( TokenRead tokenRead, params int[] properties )
		 {
			  string[] propertyKeys = new string[properties.Length];
			  for ( int i = 0; i < properties.Length; i++ )
			  {
					propertyKeys[i] = tokenRead.PropertyKeyName( properties[i] );
			  }
			  return propertyKeys;
		 }

		 public static string[] GetPropertyKeys( TokenNameLookup tokenNameLookup, LabelSchemaDescriptor descriptor )
		 {
			  int[] propertyKeyIds = descriptor.PropertyIds;
			  string[] propertyKeys = new string[propertyKeyIds.Length];
			  for ( int i = 0; i < propertyKeyIds.Length; i++ )
			  {
					propertyKeys[i] = tokenNameLookup.PropertyKeyGetName( propertyKeyIds[i] );
			  }
			  return propertyKeys;
		 }

		 public static string[] GetPropertyKeys( TokenNameLookup tokenNameLookup, int[] propertyIds )
		 {
			  string[] propertyKeys = new string[propertyIds.Length];
			  for ( int i = 0; i < propertyIds.Length; i++ )
			  {
					propertyKeys[i] = tokenNameLookup.PropertyKeyGetName( propertyIds[i] );
			  }
			  return propertyKeys;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static int[] getOrCreatePropertyKeyIds(Neo4Net.Kernel.Api.Internal.TokenWrite tokenWrite, String... propertyKeys) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IllegalTokenNameException
		 public static int[] GetOrCreatePropertyKeyIds( TokenWrite tokenWrite, params string[] propertyKeys )
		 {
			  int[] propertyKeyIds = new int[propertyKeys.Length];
			  tokenWrite.PropertyKeyGetOrCreateForNames( propertyKeys, propertyKeyIds );
			  return propertyKeyIds;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static int[] getOrCreatePropertyKeyIds(Neo4Net.Kernel.Api.Internal.TokenWrite tokenWrite, Neo4Net.GraphDb.Schema.IndexDefinition indexDefinition) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IllegalTokenNameException
		 public static int[] GetOrCreatePropertyKeyIds( TokenWrite tokenWrite, IndexDefinition indexDefinition )
		 {
			  return GetOrCreatePropertyKeyIds( tokenWrite, GetPropertyKeysArrayOf( indexDefinition ) );
		 }

		 private static string[] GetPropertyKeysArrayOf( IndexDefinition indexDefinition )
		 {
			  if ( indexDefinition is IndexDefinitionImpl )
			  {
					return ( ( IndexDefinitionImpl ) indexDefinition ).PropertyKeysArrayShared;
			  }
			  IList<string> keys = new List<string>();
			  foreach ( string key in indexDefinition.PropertyKeys )
			  {
					keys.Add( key );
			  }
			  return keys.ToArray();
		 }
	}

}