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
namespace Neo4Net.Kernel.impl.store.format.standard
{
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.impl.store.record.PropertyKeyTokenRecord;

	public class PropertyKeyTokenRecordFormat : TokenRecordFormat<PropertyKeyTokenRecord>
	{
		 public PropertyKeyTokenRecordFormat() : base(BASE_RECORD_SIZE + 4, StandardFormatSettings.PROPERTY_TOKEN_MAXIMUM_ID_BITS)
		 {
		 }

		 public override PropertyKeyTokenRecord NewRecord()
		 {
			  return new PropertyKeyTokenRecord( -1 );
		 }

		 protected internal override void ReadRecordData( PageCursor cursor, PropertyKeyTokenRecord record, bool inUse )
		 {
			  int propertyCount = cursor.Int;
			  int nameId = cursor.Int;
			  record.Initialize( inUse, nameId, propertyCount );
		 }

		 protected internal override void WriteRecordData( PropertyKeyTokenRecord record, PageCursor cursor )
		 {
			  cursor.PutInt( record.PropertyCount );
			  cursor.PutInt( record.NameId );
		 }
	}

}