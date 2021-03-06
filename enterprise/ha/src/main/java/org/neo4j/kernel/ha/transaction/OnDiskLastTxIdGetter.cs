﻿using System;

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
namespace Org.Neo4j.Kernel.ha.transaction
{
	using LastTxIdGetter = Org.Neo4j.Kernel.impl.core.LastTxIdGetter;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;

	public class OnDiskLastTxIdGetter : LastTxIdGetter
	{
		 private readonly System.Func<long> _txIdSupplier;

		 public OnDiskLastTxIdGetter( System.Func<long> txIdSupplier )
		 {
			  this._txIdSupplier = txIdSupplier;
		 }

		 /// <summary>
		 /// This method is used to construct credentials for election process.
		 /// And can be invoked at any moment of instance lifecycle.
		 /// It mean that its possible that we will be invoked when neo stores are stopped
		 /// (for example while we copy store) in that case we will return TransactionIdStore.BASE_TX_ID
		 /// </summary>
		 public virtual long LastTxId
		 {
			 get
			 {
				  try
				  {
						return _txIdSupplier.AsLong;
				  }
				  catch ( Exception )
				  {
						return Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;
				  }
			 }
		 }
	}

}