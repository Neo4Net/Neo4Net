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
namespace Neo4Net.Kernel.impl.context
{

	using VersionContext = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContext;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;

	/// <summary>
	/// Transactional version context that used by read transaction to read data of specific version.
	/// Or perform versioned data modification.
	/// </summary>
	public class TransactionVersionContext : VersionContext
	{
		 private readonly System.Func<long> _lastClosedTxIdSupplier;
		 private long _transactionId = BASE_TX_ID;
		 private long _lastClosedTxId = long.MaxValue;
		 private bool _dirty;

		 public TransactionVersionContext( System.Func<long> lastClosedTxIdSupplier )
		 {
			  this._lastClosedTxIdSupplier = lastClosedTxIdSupplier;
		 }

		 public override void InitRead()
		 {
			  long txId = _lastClosedTxIdSupplier.AsLong;
			  Debug.Assert( txId >= BASE_TX_ID );
			  _lastClosedTxId = txId;
			  _dirty = false;
		 }

		 public override void InitWrite( long committingTxId )
		 {
			  Debug.Assert( committingTxId >= BASE_TX_ID );
			  _transactionId = committingTxId;
		 }

		 public override long CommittingTransactionId()
		 {
			  return _transactionId;
		 }

		 public override long LastClosedTransactionId()
		 {
			  return _lastClosedTxId;
		 }

		 public override void MarkAsDirty()
		 {
			  _dirty = true;
		 }

		 public virtual bool Dirty
		 {
			 get
			 {
				  return _dirty;
			 }
		 }
	}

}