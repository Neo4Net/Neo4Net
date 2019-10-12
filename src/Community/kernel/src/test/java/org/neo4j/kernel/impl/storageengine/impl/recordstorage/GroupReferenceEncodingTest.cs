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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class GroupReferenceEncodingTest
	{
		 // This value the largest possible high limit id +1 (see HighLimitV3_1_0)
		 private static long _maxIdLimit = 1L << 50;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void encodeRelationship()
		 public virtual void EncodeRelationship()
		 {
			  ThreadLocalRandom random = ThreadLocalRandom.current();
			  for ( int i = 0; i < 1000; i++ )
			  {
					long reference = random.nextLong( _maxIdLimit );
					assertFalse( GroupReferenceEncoding.IsRelationship( reference ) );
					assertTrue( GroupReferenceEncoding.IsRelationship( GroupReferenceEncoding.EncodeRelationship( reference ) ) );
					assertTrue( "encoded reference is negative", GroupReferenceEncoding.EncodeRelationship( reference ) < 0 );
			  }
		 }
	}

}