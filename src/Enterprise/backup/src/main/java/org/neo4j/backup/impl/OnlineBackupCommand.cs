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
namespace Neo4Net.backup.impl
{

	using AdminCommand = Neo4Net.Commandline.Admin.AdminCommand;
	using CommandFailed = Neo4Net.Commandline.Admin.CommandFailed;
	using IncorrectUsage = Neo4Net.Commandline.Admin.IncorrectUsage;
	using OutsideWorld = Neo4Net.Commandline.Admin.OutsideWorld;

	internal class OnlineBackupCommand : AdminCommand
	{
		 private readonly OutsideWorld _outsideWorld;
		 private readonly OnlineBackupContextFactory _contextBuilder;
		 private readonly BackupStrategyCoordinatorFactory _backupStrategyCoordinatorFactory;
		 private readonly BackupSupportingClassesFactory _backupSupportingClassesFactory;

		 /// <summary>
		 /// The entry point for neo4j admin tool's online backup functionality.
		 /// </summary>
		 /// <param name="outsideWorld"> provides a way to interact with the filesystem and output streams </param>
		 /// <param name="contextBuilder"> helper class to validate, process and return a grouped result of processing the command line arguments </param>
		 /// <param name="backupSupportingClassesFactory"> necessary for constructing the strategy for backing up over the causal clustering transaction protocol </param>
		 /// <param name="backupStrategyCoordinatorFactory"> class that actually handles the logic of performing a backup </param>
		 internal OnlineBackupCommand( OutsideWorld outsideWorld, OnlineBackupContextFactory contextBuilder, BackupSupportingClassesFactory backupSupportingClassesFactory, BackupStrategyCoordinatorFactory backupStrategyCoordinatorFactory )
		 {
			  this._outsideWorld = outsideWorld;
			  this._contextBuilder = contextBuilder;
			  this._backupSupportingClassesFactory = backupSupportingClassesFactory;
			  this._backupStrategyCoordinatorFactory = backupStrategyCoordinatorFactory;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void execute(String[] args) throws org.neo4j.commandline.admin.IncorrectUsage, org.neo4j.commandline.admin.CommandFailed
		 public override void Execute( string[] args )
		 {
			  OnlineBackupContext onlineBackupContext = _contextBuilder.createContext( args );
			  ProtocolWarn( onlineBackupContext );
			  using ( BackupSupportingClasses backupSupportingClasses = _backupSupportingClassesFactory.createSupportingClasses( onlineBackupContext.Config ) )
			  {
					// Make sure destination exists
					CheckDestination( onlineBackupContext.RequiredArguments.Directory );
					CheckDestination( onlineBackupContext.RequiredArguments.ReportDir );

					BackupStrategyCoordinator backupStrategyCoordinator = _backupStrategyCoordinatorFactory.backupStrategyCoordinator( onlineBackupContext, backupSupportingClasses.BackupProtocolService, backupSupportingClasses.BackupDelegator, backupSupportingClasses.PageCache );

					backupStrategyCoordinator.PerformBackup( onlineBackupContext );
					_outsideWorld.stdOutLine( "Backup complete." );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkDestination(java.nio.file.Path path) throws org.neo4j.commandline.admin.CommandFailed
		 private void CheckDestination( Path path )
		 {
			  if ( !_outsideWorld.fileSystem().isDirectory(path.toFile()) )
			  {
					throw new CommandFailed( format( "Directory '%s' does not exist.", path ) );
			  }
		 }

		 private void ProtocolWarn( OnlineBackupContext onlineBackupContext )
		 {
			  SelectedBackupProtocol selectedBackupProtocol = onlineBackupContext.RequiredArguments.SelectedBackupProtocol;
			  if ( !SelectedBackupProtocol.Any.Equals( selectedBackupProtocol ) )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String compatibleProducts;
					string compatibleProducts;
					switch ( selectedBackupProtocol.innerEnumValue )
					{
					case Neo4Net.backup.impl.SelectedBackupProtocol.InnerEnum.CATCHUP:
						 compatibleProducts = "causal clustering";
						 break;
					case Neo4Net.backup.impl.SelectedBackupProtocol.InnerEnum.COMMON:
						 compatibleProducts = "HA and single";
						 break;
					default:
						 throw new System.ArgumentException( "Unhandled protocol " + selectedBackupProtocol );
					}
					_outsideWorld.stdOutLine( format( "The selected protocol `%s` means that it is only compatible with %s instances", selectedBackupProtocol.Name, compatibleProducts ) );
			  }
		 }
	}

}