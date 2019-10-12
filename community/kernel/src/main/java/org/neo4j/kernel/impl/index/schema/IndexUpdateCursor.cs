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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{

	using Org.Neo4j.Index.@internal.gbptree;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using UpdateMode = Org.Neo4j.Kernel.Impl.Api.index.UpdateMode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.IndexUpdateStorage.STOP_TYPE;

	/// <summary>
	/// Cursor over serialized <seealso cref="org.neo4j.kernel.api.index.IndexEntryUpdate"/> represented by <seealso cref="UpdateMode"/>, 2x<seealso cref="KEY"/> and <seealso cref="VALUE"/>.
	/// Reads the updates in sequential order. Field instances are reused, so consumer is responsible for creating copies if result needs to be cached.
	/// </summary>
	public class IndexUpdateCursor<KEY, VALUE> : BlockEntryCursor<KEY, VALUE>
	{
		 private readonly PageCursor _cursor;
		 private readonly Layout<KEY, VALUE> _layout;

		 // Fields for the last entry
		 private UpdateMode _updateMode;
		 private readonly KEY _key1;
		 private readonly KEY _key2;
		 private readonly VALUE _value;

		 internal IndexUpdateCursor( PageCursor cursor, Layout<KEY, VALUE> layout )
		 {
			  this._cursor = cursor;
			  this._layout = layout;
			  this._key1 = layout.NewKey();
			  this._key2 = layout.NewKey();
			  this._value = layout.NewValue();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws java.io.IOException
		 public override bool Next()
		 {
			  sbyte updateModeType = _cursor.Byte;
			  if ( updateModeType == STOP_TYPE )
			  {
					return false;
			  }

			  _updateMode = UpdateMode.MODES[updateModeType];
			  IndexUpdateEntry.Read( _cursor, _layout, _updateMode, _key1, _key2, _value );
			  return true;
		 }

		 public override KEY Key()
		 {
			  return _key1;
		 }

		 public override VALUE Value()
		 {
			  return _value;
		 }

		 public virtual KEY Key2()
		 {
			  return _key2;
		 }

		 public virtual UpdateMode UpdateMode()
		 {
			  return _updateMode;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _cursor.close();
		 }
	}

}