﻿using System.Threading;

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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.cache
{

	using PagedFile = Org.Neo4j.Io.pagecache.PagedFile;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class PageCacheIntArrayConcurrencyTest : PageCacheNumberArrayConcurrencyTest
	{
		 protected internal override ThreadStart WholeFileRacer( NumberArray array, int contestant )
		 {
			  return new WholeFileRacer( this, ( PageCacheIntArray ) array );
		 }

		 protected internal override ThreadStart FileRangeRacer( NumberArray array, int contestant )
		 {
			  return new FileRangeRacer( this, ( PageCacheIntArray ) array, contestant );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected PageCacheIntArray getNumberArray(org.neo4j.io.pagecache.PagedFile file) throws java.io.IOException
		 protected internal override PageCacheIntArray GetNumberArray( PagedFile file )
		 {
			  return new PageCacheIntArray( file, COUNT, 0, 0 );
		 }

		 private class WholeFileRacer : ThreadStart
		 {
			 private readonly PageCacheIntArrayConcurrencyTest _outerInstance;

			  internal IntArray Array;

			  internal WholeFileRacer( PageCacheIntArrayConcurrencyTest outerInstance, IntArray array )
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
							  Array.set( i, i );
							  assertEquals( i, Array.get( i ) );
						 }
					}
			  }
		 }

		 private class FileRangeRacer : ThreadStart
		 {
			 private readonly PageCacheIntArrayConcurrencyTest _outerInstance;

			  internal IntArray Array;
			  internal int Contestant;

			  internal FileRangeRacer( PageCacheIntArrayConcurrencyTest outerInstance, IntArray array, int contestant )
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
							  int value = outerInstance.Random.Next();
							  Array.set( i, value );
							  assertEquals( value, Array.get( i ) );
						 }
					}
			  }
		 }
	}

}