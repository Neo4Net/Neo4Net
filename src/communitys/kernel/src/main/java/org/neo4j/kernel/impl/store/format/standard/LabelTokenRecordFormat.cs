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
	using LabelTokenRecord = Neo4Net.Kernel.impl.store.record.LabelTokenRecord;

	public class LabelTokenRecordFormat : TokenRecordFormat<LabelTokenRecord>
	{
		 public LabelTokenRecordFormat() : base(BASE_RECORD_SIZE, StandardFormatSettings.LABEL_TOKEN_MAXIMUM_ID_BITS)
		 {
		 }

		 public override LabelTokenRecord NewRecord()
		 {
			  return new LabelTokenRecord( -1 );
		 }
	}

}