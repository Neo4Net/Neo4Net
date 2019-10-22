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
namespace Neo4Net.GraphDb
{

	/// <summary>
	/// Closeable Iterator with associated resources.
	/// 
	/// The associated resources are always released when the owning transaction is committed or rolled back.
	/// The resource may also be released eagerly by explicitly calling <seealso cref="org.Neo4Net.graphdb.ResourceIterator.close()"/>
	/// or by exhausting the iterator.
	/// </summary>
	/// @param <T> type of values returned by this Iterator
	/// </param>
	/// <seealso cref= ResourceIterable </seealso>
	public interface ResourceIterator<T> : IEnumerator<T>, Resource
	{
		 /// <summary>
		 /// Close the iterator early, freeing associated resources
		 /// 
		 /// It is an error to use the iterator after this has been called.
		 /// </summary>
		 void Close();

		 /// <returns> this iterator as a <seealso cref="Stream"/> </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default java.util.stream.Stream<T> stream()
	//	 {
	//		  return StreamSupport.stream(spliteratorUnknownSize(this, 0), false).onClose(this::close);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default <R> ResourceIterator<R> map(System.Func<T, R> map)
	//	 {
	//		  return new ResourceIterator<R>()
	//		  {
	//				@@Override public void close()
	//				{
	//					 ResourceIterator.this.close();
	//				}
	//
	//				@@Override public boolean hasNext()
	//				{
	//					 return ResourceIterator.this.hasNext();
	//				}
	//
	//				@@Override public R next()
	//				{
	//					 return map.apply(ResourceIterator.this.next());
	//				}
	//		  };
	//	 }
	}

}