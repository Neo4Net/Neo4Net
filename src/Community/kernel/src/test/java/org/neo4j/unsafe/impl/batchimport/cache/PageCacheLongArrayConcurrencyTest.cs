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
//	import static org.junit.Assert.assertEquals;

	public class PageCacheLongArrayConcurrencyTest : PageCacheNumberArrayConcurrencyTest
	{
		 protected internal override ThreadStart WholeFileRacer( NumberArray array, int contestant )
		 {
			  return new WholeFileRacer( this, ( PageCacheLongArray ) array );
		 }

		 protected internal override ThreadStart FileRangeRacer( NumberArray array, int contestant )
		 {
			  return new FileRangeRacer( this, ( PageCacheLongArray ) array, contestant );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected PageCacheLongArray getNumberArray(org.neo4j.io.pagecache.PagedFile file) throws java.io.IOException
		 protected internal override PageCacheLongArray GetNumberArray( PagedFile file )
		 {
			  return new PageCacheLongArray( file, COUNT, 0, 0 );
		 }

		 private class WholeFileRacer : ThreadStart
		 {
			 private readonly PageCacheLongArrayConcurrencyTest _outerInstance;

			  internal LongArray Array;

			  internal WholeFileRacer( PageCacheLongArrayConcurrencyTest outerInstance, LongArray array )
			  {
				  this._outerInstance = outerInstance;
					this.Array = array;
			  }

			  public override void Run()
			  {

					for ( int o = 0; o < LAPS; o++ )
					{
						 for ( long i = 0; i < COUNT; i++ )
						 {
							  Array.set( i, i );
							  assertEquals( i, Array.get( i ) );
						 }
					}
			  }
		 }

		 private class FileRangeRacer : ThreadStart
		 {
			 private readonly PageCacheLongArrayConcurrencyTest _outerInstance;

			  internal LongArray Array;
			  internal int Contestant;

			  internal FileRangeRacer( PageCacheLongArrayConcurrencyTest outerInstance, LongArray array, int contestant )
			  {
				  this._outerInstance = outerInstance;
					this.Array = array;
					this.Contestant = contestant;
			  }

			  public override void Run()
			  {

					for ( int o = 0; o < LAPS; o++ )
					{
						 for ( long i = Contestant; i < COUNT; i += CONTESTANTS )
						 {
							  long value = outerInstance.Random.nextLong();
							  Array.set( i, value );
							  assertEquals( value, Array.get( i ) );
						 }
					}
			  }
		 }
	}

}