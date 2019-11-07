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
namespace Neo4Net.Consistency.checking.full
{

	using Neo4Net.GraphDb;
	using Neo4Net.GraphDb;
	using Neo4Net.Collections.Helpers;
	using Neo4Net.Kernel.impl.store;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.consistency.checking.full.CloningRecordIterator.cloned;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.Scanner.scan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.record.RecordLoad.FORCE;

	public class IterableStore<RECORD> : BoundedIterable<RECORD> where RECORD : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
	{
		 private readonly RecordStore<RECORD> _store;
		 private readonly bool _forward;
		 private IResourceIterator<RECORD> _iterator;

		 public IterableStore( RecordStore<RECORD> store, bool forward )
		 {
			  this._store = store;
			  this._forward = forward;
		 }

		 public override long MaxCount()
		 {
			  return _store.HighId;
		 }

		 public override void Close()
		 {
			  CloseIterator();
		 }

		 private void CloseIterator()
		 {
			  if ( _iterator != null )
			  {
					_iterator.close();
					_iterator = null;
			  }
		 }

		 public override IEnumerator<RECORD> Iterator()
		 {
			  CloseIterator();
			 IResourceIterable<RECORD> iterable = scan( _store, _forward );
			  return cloned( _iterator = iterable.GetEnumerator() );
		 }

		 public virtual void WarmUpCache()
		 {
			  int recordsPerPage = _store.RecordsPerPage;
			  long id = 0;
			  long half = _store.HighId / 2;
			  RECORD record = _store.newRecord();
			  while ( id < half )
			  {
					_store.getRecord( id, record, FORCE );
					id += recordsPerPage - 1;
			  }
		 }
	}

}