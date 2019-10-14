using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.tools.txlog
{

	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using Neo4Net.tools.txlog.checktypes;

	/// <summary>
	/// Contains mapping from entity id (<seealso cref="NodeRecord.getId()"/>, <seealso cref="PropertyRecord.getId()"/>, ...) to
	/// <seealso cref="AbstractBaseRecord record"/> for records that have been previously seen during transaction log scan.
	/// <p/>
	/// Can determine if some given record is consistent with previously committed state.
	/// </summary>
	/// @param <R> the type of the record </param>
	internal class CommittedRecords<R> where R : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.neo4j.tools.txlog.checktypes.CheckType<?,R> checkType;
		 private readonly CheckType<object, R> _checkType;
		 private readonly IDictionary<long, RecordInfo<R>> _recordsById;

		 internal CommittedRecords<T1>( CheckType<T1> check )
		 {
			  this._checkType = check;
			  this._recordsById = new Dictionary<long, RecordInfo<R>>();
		 }

		 public virtual void Put( R record, long logVersion, long txId )
		 {
			  _recordsById[record.Id] = new RecordInfo<R>( record, logVersion, txId );
		 }

		 public virtual RecordInfo<R> Get( long id )
		 {
			  return _recordsById[id];
		 }

		 public override string ToString()
		 {
			  return "CommittedRecords{" +
						"command=" + _checkType.name() +
						", recordsById.size=" + _recordsById.Count + "}";
		 }
	}

}