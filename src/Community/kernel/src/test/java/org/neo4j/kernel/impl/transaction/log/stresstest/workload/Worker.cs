﻿using System;
using System.Threading;

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
namespace Neo4Net.Kernel.impl.transaction.log.stresstest.workload
{

	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using LogAppendEvent = Neo4Net.Kernel.impl.transaction.tracing.LogAppendEvent;

	internal class Worker : ThreadStart
	{
		 private readonly TransactionAppender _transactionAppender;
		 private readonly TransactionRepresentationFactory _factory;
		 private readonly System.Func<bool> _condition;

		 internal Worker( TransactionAppender transactionAppender, TransactionRepresentationFactory factory, System.Func<bool> condition )
		 {
			  this._transactionAppender = transactionAppender;
			  this._factory = factory;
			  this._condition = condition;
		 }

		 public override void Run()
		 {
			  long latestTxId = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;
			  while ( _condition.AsBoolean )
			  {
					TransactionToApply transaction = _factory.nextTransaction( latestTxId );
					try
					{
						 latestTxId = _transactionAppender.append( transaction, Neo4Net.Kernel.impl.transaction.tracing.LogAppendEvent_Fields.Null );
					}
					catch ( IOException e )
					{
						 throw new Exception( e );
					}
			  }
		 }
	}

}