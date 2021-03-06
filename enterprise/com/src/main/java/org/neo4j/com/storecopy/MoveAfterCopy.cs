﻿/*
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
namespace Org.Neo4j.com.storecopy
{

	public interface MoveAfterCopy
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void move(java.util.stream.Stream<FileMoveAction> moves, java.io.File fromDirectory, System.Func<java.io.File, java.io.File> destinationFunction) throws Exception;
		 void Move( Stream<FileMoveAction> moves, File fromDirectory, System.Func<File, File> destinationFunction );

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static MoveAfterCopy moveReplaceExisting()
	//	 {
	//		  return (moves, fromDirectory, destinationFunction) ->
	//		  {
	//				Iterable<FileMoveAction> itr = moves::iterator;
	//				for (FileMoveAction move : itr)
	//				{
	//					 move.move(destinationFunction.apply(move.file()), StandardCopyOption.REPLACE_EXISTING);
	//				}
	//		  };
	//	 }
	}

}