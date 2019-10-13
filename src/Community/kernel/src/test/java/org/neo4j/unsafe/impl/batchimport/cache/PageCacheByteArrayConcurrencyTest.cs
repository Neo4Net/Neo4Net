using System.Threading;

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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache
{

	using PagedFile = Neo4Net.Io.pagecache.PagedFile;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;

	public class PageCacheByteArrayConcurrencyTest : PageCacheNumberArrayConcurrencyTest
	{
		 protected internal override ThreadStart WholeFileRacer( NumberArray array, int contestant )
		 {
			  return new WholeFileRacer( this, ( ByteArray ) array );
		 }

		 protected internal override ThreadStart FileRangeRacer( NumberArray array, int contestant )
		 {
			  return new FileRangeRacer( this, ( ByteArray ) array, contestant );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected ByteArray getNumberArray(org.neo4j.io.pagecache.PagedFile file) throws java.io.IOException
		 protected internal override ByteArray GetNumberArray( PagedFile file )
		 {
			  return new PageCacheByteArray( file, COUNT, new sbyte[]{ ( sbyte ) - 1, ( sbyte ) - 1, ( sbyte ) - 1, ( sbyte ) - 1 }, 0 );
		 }

		 private class WholeFileRacer : ThreadStart
		 {
			 private readonly PageCacheByteArrayConcurrencyTest _outerInstance;

			  internal ByteArray Array;

			  internal WholeFileRacer( PageCacheByteArrayConcurrencyTest outerInstance, ByteArray array )
			  {
				  this._outerInstance = outerInstance;
					this.Array = array;
			  }

			  public override void Run()
			  {
					for ( int o = 0; o < LAPS; o++ )
					{
						 for ( int i = 0; i < COUNT; i++ )
						 {
							  sbyte[] value = new sbyte[] { 1, 2, 3, 4 };
							  Array.set( i, value );
							  sbyte[] actual = new sbyte[4];
							  Array.get( i, actual );
							  assertArrayEquals( value, actual );
						 }
					}
			  }
		 }

		 private class FileRangeRacer : ThreadStart
		 {
			 private readonly PageCacheByteArrayConcurrencyTest _outerInstance;

			  internal ByteArray Array;
			  internal int Contestant;

			  internal FileRangeRacer( PageCacheByteArrayConcurrencyTest outerInstance, ByteArray array, int contestant )
			  {
				  this._outerInstance = outerInstance;
					this.Array = array;
					this.Contestant = contestant;
			  }

			  public override void Run()
			  {
					for ( int o = 0; o < LAPS; o++ )
					{
						 for ( int i = Contestant; i < COUNT; i += CONTESTANTS )
						 {
							  sbyte[] value = new sbyte[4];
							  sbyte[] actual = new sbyte[4];
							  outerInstance.Random.NextBytes( value );
							  Array.set( i, value );
							  Array.get( i, actual );
							  assertArrayEquals( value, actual );
						 }
					}
			  }
		 }
	}

}