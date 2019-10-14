using System;

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
namespace Neo4Net.Kernel.impl.locking
{
	using MutableLongLongMap = org.eclipse.collections.api.map.primitive.MutableLongLongMap;
	using LongLongHashMap = org.eclipse.collections.impl.map.mutable.primitive.LongLongHashMap;
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.IndexQuery.exact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.locking.ResourceTypes.indexEntryResourceId;

	/// <summary>
	/// This is an *IT integration test because it uses a large amount of memory.
	/// Approximately 1.2 GBs goes into the map we use to check for collisions.
	/// </summary>
	public class ResourceTypesIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexEntryHashing()
		 public virtual void IndexEntryHashing()
		 {
			  int collisions = 0;
			  int labelIdCount = 50;
			  int propertyKeyIdCount = 50;
			  int objectCount = 10000;
			  MutableLongLongMap map = new LongLongHashMap( 50 * 50 * 10000 );
			  string[] values = PrecomputeValues( objectCount );

			  for ( int labelId = 0; labelId < labelIdCount; labelId++ )
			  {
					for ( int propertyKeyId = 0; propertyKeyId < propertyKeyIdCount; propertyKeyId++ )
					{
						 for ( int objectId = 0; objectId < objectCount; objectId++ )
						 {
							  string @object = values[objectId];
							  long resourceId = indexEntryResourceId( labelId, exact( propertyKeyId, @object ) );

							  long newValue = PackValue( labelId, propertyKeyId, objectId );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean hasOldValue = map.containsKey(resourceId);
							  bool hasOldValue = map.containsKey( resourceId );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long oldValue = map.get(resourceId);
							  long oldValue = map.get( resourceId );
							  map.put( resourceId, newValue );
							  if ( hasOldValue )
							  {
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: System.out.printf("Collision on %s: %s ~= %s%n", resourceId, toValueString(newValue), toValueString(oldValue));
									Console.Write( "Collision on %s: %s ~= %s%n", resourceId, ToValueString( newValue ), ToValueString( oldValue ) );
									collisions++;
									if ( collisions > 100 )
									{
										 fail( "This hashing is terrible!" );
									}
							  }
						 }
					}
			  }

			  assertThat( collisions, @is( 0 ) );
		 }

		 private long PackValue( int labelId, int propertyKeyId, int objectId )
		 {
			  long result = labelId;
			  result <<= 16;
			  result += propertyKeyId;
			  result <<= 32;
			  result += objectId;
			  return result;
		 }

		 private string ToValueString( long value )
		 {
			  int objectId = unchecked( ( int )( value & 0x00000000_FFFFFFFFL ) );
			  int propertyKeyId = ( int )( ( long )( ( ulong )( value & 0x0000FFFF_00000000L ) >> 32 ) );
			  int labelId = ( int )( ( int )( ( uint )( value & 0xFFFF0000_00000000L ) >> 48 ) );
			  return string.Format( "IndexEntry{{ labelId={0}, propertyKeyId={1}, objectId={2} }}", labelId, propertyKeyId, objectId );
		 }

		 private string[] PrecomputeValues( int objectCount )
		 {
			  string[] values = new string[objectCount];
			  for ( int i = 0; i < objectCount; i++ )
			  {
					values[i] = "" + i;
			  }
			  return values;
		 }
	}

}