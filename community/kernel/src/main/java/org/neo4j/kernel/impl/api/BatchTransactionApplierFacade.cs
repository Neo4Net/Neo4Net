﻿/*
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
namespace Org.Neo4j.Kernel.Impl.Api
{

	using LockGroup = Org.Neo4j.Kernel.impl.locking.LockGroup;
	using CommandsToApply = Org.Neo4j.Storageengine.Api.CommandsToApply;

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
//ORIGINAL LINE: public TransactionApplier startTx(org.neo4j.storageengine.api.CommandsToApply transaction) throws java.io.IOException
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
//ORIGINAL LINE: public TransactionApplier startTx(org.neo4j.storageengine.api.CommandsToApply transaction, org.neo4j.kernel.impl.locking.LockGroup lockGroup) throws java.io.IOException
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