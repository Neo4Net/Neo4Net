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
namespace Neo4Net.Kernel.Impl.Api
{

	using LockGroup = Neo4Net.Kernel.impl.locking.LockGroup;
	using CommandsToApply = Neo4Net.Kernel.Api.StorageEngine.CommandsToApply;

	/// <summary>
	/// This class wraps several <seealso cref="BatchTransactionApplier"/>s which will do their work sequentially. See also {@link
	/// TransactionApplierFacade} which is used to wrap the <seealso cref="startTx(CommandsToApply)"/> and {@link
	/// #startTx(CommandsToApply, LockGroup)} methods.
	/// </summary>
	public class BatchTransactionApplierFacade : BatchTransactionApplier
	{

		 private readonly BatchTransactionApplier[] _appliers;

		 public BatchTransactionApplierFacade( params BatchTransactionApplier[] appliers )
		 {
			  this._appliers = appliers;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public TransactionApplier startTx(org.Neo4Net.Kernel.Api.StorageEngine.CommandsToApply transaction) throws java.io.IOException
		 public override TransactionApplier StartTx( CommandsToApply transaction )
		 {
			  TransactionApplier[] txAppliers = new TransactionApplier[_appliers.Length];
			  for ( int i = 0; i < _appliers.Length; i++ )
			  {
					txAppliers[i] = _appliers[i].startTx( transaction );
			  }
			  return new TransactionApplierFacade( txAppliers );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public TransactionApplier startTx(org.Neo4Net.Kernel.Api.StorageEngine.CommandsToApply transaction, org.Neo4Net.kernel.impl.locking.LockGroup lockGroup) throws java.io.IOException
		 public override TransactionApplier StartTx( CommandsToApply transaction, LockGroup lockGroup )
		 {
			  TransactionApplier[] txAppliers = new TransactionApplier[_appliers.Length];
			  for ( int i = 0; i < _appliers.Length; i++ )
			  {
					txAppliers[i] = _appliers[i].startTx( transaction, lockGroup );
			  }
			  return new TransactionApplierFacade( txAppliers );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		 public override void Close()
		 {
			  // Not sure why it is necessary to close them in reverse order
			  for ( int i = _appliers.Length; i-- > 0; )
			  {
					_appliers[i].close();
			  }
		 }
	}

}