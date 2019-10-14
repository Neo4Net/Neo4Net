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
namespace Neo4Net.Kernel.impl.transaction.log
{

	using Neo4Net.Cursors;

	/// <summary>
	/// <seealso cref="IOCursor"/> implementation that uses a predicate to decide on what to keep and what to skip
	/// </summary>
	public class FilteringIOCursor<T> : IOCursor<T>
	{
		 private readonly IOCursor<T> @delegate;
		 private readonly System.Predicate<T> _toKeep;

		 public FilteringIOCursor( IOCursor<T> @delegate, System.Predicate<T> toKeep )
		 {
			  this.@delegate = @delegate;
			  this._toKeep = toKeep;
		 }

		 public override T Get()
		 {
			  return @delegate.get();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws java.io.IOException
		 public override bool Next()
		 {
			  do
			  {
					if ( !@delegate.next() )
					{
						 return false;
					}
			  } while ( !_toKeep.test( @delegate.get() ) );
			  return true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  @delegate.close();
		 }
	}

}