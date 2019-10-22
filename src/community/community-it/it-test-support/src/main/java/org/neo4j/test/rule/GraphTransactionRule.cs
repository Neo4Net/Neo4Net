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
namespace Neo4Net.Test.rule
{
	using ExternalResource = org.junit.rules.ExternalResource;

	using Transaction = Neo4Net.GraphDb.Transaction;

	/// <summary>
	/// JUnit @Rule for running a transaction for the duration of a test. Requires an EmbeddedDatabaseRule with
	/// whose database the transaction will be executed.
	/// </summary>
	public class GraphTransactionRule : ExternalResource
	{
		 private readonly DatabaseRule _database;

		 private Transaction _tx;

		 public GraphTransactionRule( DatabaseRule database )
		 {
			  this._database = database;
		 }

		 protected internal override void Before()
		 {
			  Begin();
		 }

		 protected internal override void After()
		 {
			  Success();
		 }

		 public virtual Transaction Current()
		 {
			  return _tx;
		 }

		 public virtual Transaction Begin()
		 {
			  _tx = _database.GraphDatabaseAPI.beginTx();
			  return _tx;
		 }

		 public virtual void Success()
		 {
			  try
			  {
					if ( _tx != null )
					{
						 _tx.success();
						 _tx.close();
					}
			  }
			  finally
			  {
					_tx = null;
			  }
		 }

		 public virtual void Failure()
		 {
			  try
			  {
					if ( _tx != null )
					{
						 _tx.failure();
						 _tx.close();
					}
			  }
			  finally
			  {
					_tx = null;
			  }
		 }
	}

}