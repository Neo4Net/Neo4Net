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
namespace Org.Neo4j.backup.impl
{
	using StoreCopyClientMonitor = Org.Neo4j.com.storecopy.StoreCopyClientMonitor;
	using OutsideWorld = Org.Neo4j.Commandline.admin.OutsideWorld;
	using FormattedLogProvider = Org.Neo4j.Logging.FormattedLogProvider;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	/// <summary>
	/// Monitor for events that should be displayed to neo4j-admin backup stdout
	/// </summary>
	internal class BackupOutputMonitor : StoreCopyClientMonitor
	{
		 private readonly Log _log;

		 internal BackupOutputMonitor( OutsideWorld outsideWorld )
		 {
			  LogProvider stdOutLogProvider = FormattedLogProvider.toOutputStream( outsideWorld.OutStream() );
			  _log = stdOutLogProvider.GetLog( typeof( BackupOutputMonitor ) );
		 }

		 public override void StartReceivingStoreFiles()
		 {
			  _log.info( "Start receiving store files" );
		 }

		 public override void FinishReceivingStoreFiles()
		 {
			  _log.info( "Finish receiving store files" );
		 }

		 public override void StartReceivingStoreFile( string file )
		 {
			  _log.info( "Start receiving store file %s", file );
		 }

		 public override void FinishReceivingStoreFile( string file )
		 {
			  _log.info( "Finish receiving store file %s", file );
		 }

		 public override void StartReceivingTransactions( long startTxId )
		 {
			  _log.info( "Start receiving transactions from %d", startTxId );
		 }

		 public override void FinishReceivingTransactions( long endTxId )
		 {
			  _log.info( "Finish receiving transactions at %d", endTxId );
		 }

		 public override void StartRecoveringStore()
		 {
			  _log.info( "Start recovering store" );
		 }

		 public override void FinishRecoveringStore()
		 {
			  _log.info( "Finish recovering store" );
		 }

		 public override void StartReceivingIndexSnapshots()
		 {
			  _log.info( "Start receiving index snapshots" );
		 }

		 public override void StartReceivingIndexSnapshot( long indexId )
		 {
			  _log.info( "Start receiving index snapshot id %d", indexId );
		 }

		 public override void FinishReceivingIndexSnapshot( long indexId )
		 {
			  _log.info( "Finished receiving index snapshot id %d", indexId );
		 }

		 public override void FinishReceivingIndexSnapshots()
		 {
			  _log.info( "Finished receiving index snapshots" );
		 }
	}

}