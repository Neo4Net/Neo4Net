/*
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
namespace Org.Neo4j.tools.txlog
{
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;

	/// <summary>
	/// Represents a record (<seealso cref="NodeRecord"/>, <seealso cref="PropertyRecord"/>, ...) from a transaction log file with some
	/// particular version.
	/// </summary>
	/// @param <R> the type of the record </param>
	public class RecordInfo<R> where R : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
	{
		 private readonly R _record;
		 private readonly long _logVersion;
		 private readonly long _txId;

		 public RecordInfo( R record, long logVersion, long txId )
		 {
			  this._record = record;
			  this._logVersion = logVersion;
			  this._txId = txId;
		 }

		 public virtual R Record()
		 {
			  return _record;
		 }

		 public virtual long TxId()
		 {
			  return _txId;
		 }

		 internal virtual long LogVersion()
		 {
			  return _logVersion;
		 }

		 public override string ToString()
		 {
			  return string.Format( "{0} (log:{1:D} txId:{2:D})", _record, _logVersion, _txId );
		 }
	}

}