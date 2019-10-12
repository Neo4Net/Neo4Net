using System.Text;

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
namespace Org.Neo4j.@internal.Kernel.Api.schema
{

	public class SchemaUtil
	{
		 private SchemaUtil()
		 {
		 }

		 public static string NiceProperties( TokenNameLookup tokenNameLookup, int[] propertyIds )
		 {
			  return NiceProperties( tokenNameLookup, propertyIds, "", false );
		 }

		 public static string NiceProperties( TokenNameLookup tokenNameLookup, int[] propertyIds, string prefix, bool useBrackets )
		 {
			  StringBuilder properties = new StringBuilder();
			  if ( useBrackets )
			  {
					properties.Append( "(" );
			  }
			  for ( int i = 0; i < propertyIds.Length; i++ )
			  {
					if ( i > 0 )
					{
						 properties.Append( ", " );
					}
					properties.Append( prefix ).Append( tokenNameLookup.PropertyKeyGetName( propertyIds[i] ) );
			  }
			  if ( useBrackets )
			  {
					properties.Append( ")" );
			  }
			  return properties.ToString();
		 }

		 public static readonly TokenNameLookup idTokenNameLookup = new TokenNameLookupAnonymousInnerClass();

		 private class TokenNameLookupAnonymousInnerClass : TokenNameLookup
		 {

			 public string labelGetName( int labelId )
			 {
				  return format( "label[%d]", labelId );
			 }

			 public string relationshipTypeGetName( int relationshipTypeId )
			 {
				  return format( "relType[%d]", relationshipTypeId );
			 }

			 public string propertyKeyGetName( int propertyKeyId )
			 {
				  return format( "property[%d]", propertyKeyId );
			 }
		 }
	}

}