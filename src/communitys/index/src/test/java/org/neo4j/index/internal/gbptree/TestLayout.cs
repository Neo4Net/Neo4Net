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
namespace Neo4Net.Index.@internal.gbptree
{
	internal abstract class TestLayout<KEY, VALUE> : Layout_Adapter<KEY, VALUE>, KeyValueSeeder<KEY, VALUE>
	{
		public abstract long ValueSeed( VALUE value );
		public abstract long KeySeed( KEY key );
		public abstract VALUE Value( long seed );
		public abstract KEY Key( long seed );
		public override abstract long NamedIdentifier( string name, int identifier );
		public override abstract void ReadMetaData( Neo4Net.Io.pagecache.PageCursor cursor );
		public override abstract void WriteMetaData( Neo4Net.Io.pagecache.PageCursor cursor );
		public override abstract int MinorVersion();
		public override abstract int MajorVersion();
		public override abstract long Identifier();
		public override abstract void MinimalSplitter( KEY left, KEY right, KEY into );
		public override abstract bool FixedSize();
		public override abstract void ReadValue( Neo4Net.Io.pagecache.PageCursor cursor, VALUE into, int valueSize );
		public override abstract void ReadKey( Neo4Net.Io.pagecache.PageCursor cursor, KEY into, int keySize );
		public override abstract void WriteValue( Neo4Net.Io.pagecache.PageCursor cursor, VALUE value );
		public override abstract void WriteKey( Neo4Net.Io.pagecache.PageCursor cursor, KEY key );
		public override abstract int ValueSize( VALUE value );
		public override abstract int KeySize( KEY key );
		public override abstract VALUE NewValue();
		public override abstract KEY CopyKey( KEY key, KEY into );
		public override abstract KEY NewKey();
		 internal abstract int CompareValue( VALUE v1, VALUE v2 );
	}

}