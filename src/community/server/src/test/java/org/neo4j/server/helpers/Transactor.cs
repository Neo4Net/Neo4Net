using System;
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
namespace Neo4Net.Server.helpers
{
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Transaction = Neo4Net.GraphDb.Transaction;

	public class Transactor
	{

		 private readonly Neo4Net.Server.helpers.UnitOfWork _unitOfWork;
		 private readonly IGraphDatabaseService _graphDb;
		 private readonly int _attempts; // how many times to try, if the transaction fails for some reason

		 public Transactor( IGraphDatabaseService graphDb, UnitOfWork unitOfWork ) : this( graphDb, unitOfWork, 1 )
		 {
		 }

		 public Transactor( IGraphDatabaseService graphDb, UnitOfWork unitOfWork, int attempts )
		 {
			  Debug.Assert( attempts > 0, "The Transactor should make at least one attempt at running the transaction." );
			  this._unitOfWork = unitOfWork;
			  this._graphDb = graphDb;
			  this._attempts = attempts;
		 }

		 public virtual void Execute()
		 {
			  for ( int attemptsLeft = _attempts - 1; attemptsLeft >= 0; attemptsLeft-- )
			  {
					try
					{
							using ( Transaction tx = _graphDb.beginTx() )
							{
							 _unitOfWork.doWork();
							 tx.Success();
							}
					}
					catch ( Exception e )
					{
						 if ( attemptsLeft == 0 )
						 {
							  throw e;
						 }
					}
			  }
		 }

	}

}