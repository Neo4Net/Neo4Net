﻿using System;
using System.Threading;

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
namespace Org.Neo4j.causalclustering.stresstests
{

	using DatabaseShutdownException = Org.Neo4j.Graphdb.DatabaseShutdownException;
	using TransactionFailureException = Org.Neo4j.Graphdb.TransactionFailureException;
	using TransientTransactionFailureException = Org.Neo4j.Graphdb.TransientTransactionFailureException;
	using AcquireLockTimeoutException = Org.Neo4j.Storageengine.Api.@lock.AcquireLockTimeoutException;

	internal class TxHelp
	{
		 internal static bool IsTransient( Exception e )
		 {
			  return e != null && ( e is TimeoutException || e is DatabaseShutdownException || e is TransactionFailureException || e is AcquireLockTimeoutException || e is TransientTransactionFailureException || IsInterrupted( e.InnerException ) );
		 }

		 internal static bool IsInterrupted( Exception e )
		 {
			  if ( e == null )
			  {
					return false;
			  }

			  if ( e is InterruptedException )
			  {
					Thread.interrupted();
					return true;
			  }

			  return IsInterrupted( e.InnerException );
		 }
	}

}