﻿/*
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
namespace Neo4Net.Server.rest.transactional
{
	public class DisabledTransactionRegistry : TransactionRegistry
	{
		 public static readonly TransactionRegistry Instance = new DisabledTransactionRegistry();

		 private DisabledTransactionRegistry()
		 {
		 }

		 public override long Begin( TransactionHandle handle )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override long Release( long id, TransactionHandle transactionHandle )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override TransactionHandle Acquire( long id )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void Forget( long id )
		 {
		 }

		 public override TransactionHandle Terminate( long id )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void RollbackAllSuspendedTransactions()
		 {
		 }
	}

}