﻿/*
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
namespace Org.Neo4j.Bolt.v1.runtime.spi
{
	using BoltResult = Org.Neo4j.Bolt.runtime.BoltResult;
	using Bookmark = Org.Neo4j.Bolt.v1.runtime.bookmarking.Bookmark;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

	public class BookmarkResult : BoltResult
	{
		 private readonly Bookmark _bookmark;

		 public BookmarkResult( Bookmark bookmark )
		 {
			  this._bookmark = bookmark;
		 }

		 public override string[] FieldNames()
		 {
			  return new string[0];
		 }

		 public override void Accept( Org.Neo4j.Bolt.runtime.BoltResult_Visitor visitor )
		 {
			  visitor.AddMetadata( "bookmark", stringValue( _bookmark.ToString() ) );
		 }

		 public override void Close()
		 {
		 }

		 public override string ToString()
		 {
			  return "BookmarkResult{" + "bookmark=" + _bookmark + '}';
		 }
	}

}