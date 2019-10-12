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

	using TransactionLogCatchUpFactory = Org.Neo4j.causalclustering.catchup.tx.TransactionLogCatchUpFactory;
	using OutsideWorld = Org.Neo4j.Commandline.admin.OutsideWorld;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitialisedScheduler;

	public class BackupModule
	{
		 private readonly OutsideWorld _outsideWorld;
		 private readonly LogProvider _logProvider;
		 private readonly FileSystemAbstraction _fileSystemAbstraction;
		 private readonly Monitors _monitors;
		 private readonly Clock _clock;
		 private readonly TransactionLogCatchUpFactory _transactionLogCatchUpFactory;
		 private readonly JobScheduler _jobScheduler;

		 /// <summary>
		 /// Dependencies that can be resolved immediately after launching the backup tool
		 /// </summary>
		 /// <param name="outsideWorld"> filesystem and output streams that the tool interacts with </param>
		 /// <param name="logProvider"> made available to subsequent dependency resolution classes </param>
		 /// <param name="monitors"> will become shared across all resolved dependencies </param>
		 internal BackupModule( OutsideWorld outsideWorld, LogProvider logProvider, Monitors monitors )
		 {
			  this._outsideWorld = outsideWorld;
			  this._logProvider = logProvider;
			  this._monitors = monitors;
			  this._clock = Clock.systemDefaultZone();
			  this._transactionLogCatchUpFactory = new TransactionLogCatchUpFactory();
			  this._fileSystemAbstraction = outsideWorld.FileSystem();
			  this._jobScheduler = createInitialisedScheduler();
		 }

		 public virtual LogProvider LogProvider
		 {
			 get
			 {
				  return _logProvider;
			 }
		 }

		 public virtual FileSystemAbstraction FileSystemAbstraction
		 {
			 get
			 {
				  return _fileSystemAbstraction;
			 }
		 }

		 public virtual Monitors Monitors
		 {
			 get
			 {
				  return _monitors;
			 }
		 }

		 public virtual Clock Clock
		 {
			 get
			 {
				  return _clock;
			 }
		 }

		 public virtual TransactionLogCatchUpFactory TransactionLogCatchUpFactory
		 {
			 get
			 {
				  return _transactionLogCatchUpFactory;
			 }
		 }

		 public virtual OutsideWorld OutsideWorld
		 {
			 get
			 {
				  return _outsideWorld;
			 }
		 }

		 public virtual JobScheduler JobScheduler()
		 {
			  return _jobScheduler;
		 }
	}

}