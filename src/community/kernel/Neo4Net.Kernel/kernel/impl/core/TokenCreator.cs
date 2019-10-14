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
namespace Neo4Net.Kernel.impl.core
{

	using KernelException = Neo4Net.Internal.Kernel.Api.exceptions.KernelException;

	public interface TokenCreator
	{
		 /// <summary>
		 /// Create a token by the given name and return the newly allocated id for this token.
		 /// <para>
		 /// It is assumed that the token name is not already being used.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="name"> The token name to allocate. </param>
		 /// <returns> The id of the allocated token name. </returns>
		 /// <exception cref="KernelException"> If the inner transaction used to allocate the token encountered a problem. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: int createToken(String name) throws org.neo4j.internal.kernel.api.exceptions.KernelException;
		 int CreateToken( string name );

		 /// <summary>
		 /// Create the tokens by the given names, and store their ids in the corresponding entry in the {@code ids} array,
		 /// but only if the {@code indexFilter} returns {@code true} for the given index.
		 /// </summary>
		 /// <param name="names"> The array of token names we potentially want to create new ids for. </param>
		 /// <param name="ids"> The array into which we still store the id we create for the various token names. </param>
		 /// <param name="indexFilter"> A filter for the array indexes for which a token needs an id. </param>
		 /// <exception cref="KernelException"> If the inner transaction used to allocate the tokens encountered a problem. </exception>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void createTokens(String[] names, int[] ids, System.Func<int, boolean> indexFilter) throws org.neo4j.Internal.kernel.api.exceptions.KernelException
	//	 {
	//		  for (int i = 0; i < ids.length; i++)
	//		  {
	//				if (indexFilter.test(i))
	//				{
	//					 ids[i] = createToken(names[i]);
	//				}
	//		  }
	//	 }
	}

}