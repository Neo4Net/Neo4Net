using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.impl.transaction.command
{

	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using PropertyBlock = Neo4Net.Kernel.Impl.Store.Records.PropertyBlock;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using TokenRecord = Neo4Net.Kernel.Impl.Store.Records.TokenRecord;

	public abstract class CommandReading
	{
		 public interface DynamicRecordAdder<T>
		 {
			  void Add( T target, DynamicRecord record );
		 }

		 public static readonly DynamicRecordAdder<PropertyBlock> PropertyBlockDynamicRecordAdder = ( target, record ) =>
		 {
		  record.setCreated();
		  target.addValueRecord( record );
		 };

		 public static readonly DynamicRecordAdder<ICollection<DynamicRecord>> CollectionDynamicRecordAdder = System.Collections.ICollection.add;

		 public static readonly DynamicRecordAdder<PropertyRecord> PropertyDeletedDynamicRecordAdder = ( target, record ) =>
		 {
		  Debug.Assert( !record.inUse(), record + " is kinda weird" );
		  target.addDeletedRecord( record );
		 };

		 public static readonly DynamicRecordAdder<PropertyKeyTokenRecord> PropertyIndexDynamicRecordAdder = TokenRecord.addNameRecord;
	}

}