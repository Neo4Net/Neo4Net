using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.com
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class ToAssertionWriter : MadeUpWriter
	{
		 private int _index;

		 public override void Write( ReadableByteChannel data )
		 {
			  ByteBuffer intermediate = ByteBuffer.allocate( 1000 );
			  while ( true )
			  {
					try
					{
						 intermediate.clear();
						 if ( data.read( intermediate ) == -1 )
						 {
							  break;
						 }
						 intermediate.flip();
						 while ( intermediate.remaining() > 0 )
						 {
							  sbyte value = intermediate.get();
							  assertEquals( ( _index++ ) % 10, value );
						 }
					}
					catch ( IOException e )
					{
						 throw new Exception( e );
					}
			  }
		 }
	}

}