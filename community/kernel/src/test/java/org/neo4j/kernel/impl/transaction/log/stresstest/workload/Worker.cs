using System;
using System.Threading;

/*
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
namespace Org.Neo4j.Kernel.impl.transaction.log.stresstest.workload
{

	using TransactionToApply = Org.Neo4j.Kernel.Impl.Api.TransactionToApply;
	using LogAppendEvent = Org.Neo4j.Kernel.impl.transaction.tracing.LogAppendEvent;

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
			  long latestTxId = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;
			  while ( _condition.AsBoolean )
			  {
					TransactionToApply transaction = _factory.nextTransaction( latestTxId );
					try
					{
						 latestTxId = _transactionAppender.append( transaction, Org.Neo4j.Kernel.impl.transaction.tracing.LogAppendEvent_Fields.Null );
					}
					catch ( IOException e )
					{
						 throw new Exception( e );
					}
			  }
		 }
	}

}