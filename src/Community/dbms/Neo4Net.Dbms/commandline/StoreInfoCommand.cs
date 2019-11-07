using System;

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
namespace Neo4Net.Dbms.CommandLine
{

	using AdminCommand = Neo4Net.CommandLine.Admin.AdminCommand;
	using CommandFailed = Neo4Net.CommandLine.Admin.CommandFailed;
	using IncorrectUsage = Neo4Net.CommandLine.Admin.IncorrectUsage;
	using Arguments = Neo4Net.CommandLine.Args.Arguments;
	using MandatoryCanonicalPath = Neo4Net.CommandLine.Args.Common.MandatoryCanonicalPath;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using StandalonePageCacheFactory = Neo4Net.Io.pagecache.impl.muninn.StandalonePageCacheFactory;
	using StoreLockException = Neo4Net.Kernel.StoreLockException;
	using RecordFormatSelector = Neo4Net.Kernel.impl.store.format.RecordFormatSelector;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using StoreVersionCheck = Neo4Net.Kernel.impl.storemigration.StoreVersionCheck;
	using Validators = Neo4Net.Kernel.impl.util.Validators;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.scheduler.JobSchedulerFactory.createInitializedScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.format.RecordFormatSelector.findSuccessor;

	public class StoreInfoCommand : AdminCommand
	{
		 private static readonly Arguments _arguments = new Arguments().withArgument(new MandatoryCanonicalPath("store", "path-to-dir", "Path to database store."));

		 private System.Action<string> @out;

		 public StoreInfoCommand( System.Action<string> @out )
		 {
			  this.@out = @out;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void execute(String[] args) throws Neo4Net.commandline.admin.IncorrectUsage, Neo4Net.commandline.admin.CommandFailed
		 public override void Execute( string[] args )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.nio.file.Path databaseDirectory = arguments.parse(args).getMandatoryPath("store");
			  Path databaseDirectory = _arguments.parse( args ).getMandatoryPath( "store" );

			  Validators.CONTAINS_EXISTING_DATABASE.validate( databaseDirectory.toFile() );

			  DatabaseLayout databaseLayout = DatabaseLayout.of( databaseDirectory.toFile() );
			  try
			  {
					  using ( System.IDisposable ignored = StoreLockChecker.Check( databaseLayout.StoreLayout ), DefaultFileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction(), IJobScheduler jobScheduler = createInitializedScheduler(), PageCache pageCache = StandalonePageCacheFactory.createPageCache(fileSystem, jobScheduler) )
					  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String storeVersion = new Neo4Net.kernel.impl.storemigration.StoreVersionCheck(pageCache).getVersion(databaseLayout.metadataStore()).orElseThrow(() -> new Neo4Net.commandline.admin.CommandFailed(String.format("Could not find version metadata in store '%s'", databaseDirectory)));
						string storeVersion = ( new StoreVersionCheck( pageCache ) ).getVersion( databaseLayout.MetadataStore() ).orElseThrow(() => new CommandFailed(string.Format("Could not find version metadata in store '{0}'", databaseDirectory)));
      
						const string fmt = "%-30s%s";
						@out.accept( string.format( fmt, "Store format version:", storeVersion ) );
      
						RecordFormats format = RecordFormatSelector.selectForVersion( storeVersion );
						@out.accept( string.format( fmt, "Store format introduced in:", format.IntroductionVersion() ) );
      
						findSuccessor( format ).map( next => string.format( fmt, "Store format superseded in:", next.introductionVersion() ) ).ifPresent(@out);
					  }
			  }
			  catch ( StoreLockException e )
			  {
					throw new CommandFailed( "the database is in use -- stop Neo4Net and try again", e );
			  }
			  catch ( Exception e )
			  {
					throw new CommandFailed( e.Message, e );
			  }
		 }

		 public static Arguments Arguments()
		 {
			  return _arguments;
		 }
	}

}