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
namespace Neo4Net.Kernel.ha
{

	using TxChecksumVerifier = Neo4Net.com.TxChecksumVerifier;
	using NoSuchTransactionException = Neo4Net.Kernel.impl.transaction.log.NoSuchTransactionException;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	/// <summary>
	/// Used on the master to verify that slaves are using the same logical database as the master is running. This is done
	/// by verifying transaction checksums.
	/// </summary>
	public class BranchDetectingTxVerifier : TxChecksumVerifier
	{
		 private readonly Log _log;
		 private readonly TransactionChecksumLookup _txChecksumLookup;

		 public BranchDetectingTxVerifier( LogProvider logProvider, TransactionChecksumLookup txChecksumLookup )
		 {
			  this._log = logProvider.getLog( this.GetType() );
			  this._txChecksumLookup = txChecksumLookup;
		 }

		 public override void AssertMatch( long txId, long checksum )
		 {
			  if ( txId == 0 )
			  {
					return;
			  }
			  long readChecksum;
			  try
			  {
					readChecksum = _txChecksumLookup.lookup( txId );
			  }
			  catch ( NoSuchTransactionException )
			  {
					// This can happen if it's the first commit from a slave in a cluster where all instances
					// just restored from backup (i.e. no previous tx logs exist), OR if a reporting instance
					// is so far behind that logs have been pruned to the point where the slave cannot catch up anymore.
					// In the first case it's fine (slave had to do checksum match to join cluster), and the second case
					// it's an operational issue solved by making sure enough logs are retained.

					return; // Ok!
			  }
			  catch ( IOException e )
			  {
					_log.error( "Couldn't verify checksum for " + Stringify( txId, checksum ), e );
					throw new BranchedDataException( "Unable to perform a mandatory sanity check due to an IO error.", e );
			  }

			  if ( checksum != readChecksum )
			  {
					throw new BranchedDataException( "The cluster contains two logically different versions of the database. " + "This will be automatically resolved. Details: " + Stringify( txId, checksum ) + " does not match " + readChecksum );
			  }
		 }

		 private string Stringify( long txId, long checksum )
		 {
			  return "txId:" + txId + ", checksum:" + checksum;
		 }
	}

}