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
namespace Neo4Net.Bolt.v1.runtime.spi
{
	using BoltResult = Neo4Net.Bolt.runtime.BoltResult;
	using Bookmark = Neo4Net.Bolt.v1.runtime.bookmarking.Bookmark;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.stringValue;

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

		 public override void Accept( Neo4Net.Bolt.runtime.BoltResult_Visitor visitor )
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