using System;
using System.Collections.Generic;
using System.IO;

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
	using StringUtils = org.apache.commons.lang3.StringUtils;


	using CommandFailed = Org.Neo4j.Commandline.admin.CommandFailed;
	using IncorrectUsage = Org.Neo4j.Commandline.admin.IncorrectUsage;
	using OutsideWorld = Org.Neo4j.Commandline.admin.OutsideWorld;
	using ParameterisedOutsideWorld = Org.Neo4j.Commandline.admin.ParameterisedOutsideWorld;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using Config = Org.Neo4j.Kernel.configuration.Config;

	/// <summary>
	/// This class encapsulates the information needed to perform an online backup against a running Neo4j instance
	/// configured to act as a backup server.
	/// <para>
	/// All backup methods return the same instance, allowing for chaining calls.
	/// </para>
	/// </summary>
	public class OnlineBackupCommandBuilder
	{
		 private string _host;
		 private int? _port;
		 private bool? _fallbackToFull;
		 private long? _timeout;
		 private bool? _checkConsistency;
		 private File _consistencyReportLocation;
		 private Config _additionalConfig;
		 private SelectedBackupProtocol _selectedBackupProtocol;
		 private bool? _consistencyCheckGraph;
		 private bool? _consistencyCheckIndexes;
		 private bool? _consistencyCheckLabel;
		 private bool? _consistencyCheckOwners;
		 private Stream _output;
		 private Optional<string[]> _rawArgs = null;

		 public virtual OnlineBackupCommandBuilder WithRawArgs( params string[] args )
		 {
			  _rawArgs = args;
			  return this;
		 }

		 public virtual OnlineBackupCommandBuilder WithHost( string host )
		 {
			  this._host = host;
			  return this;
		 }

		 public virtual OnlineBackupCommandBuilder WithPort( int? port )
		 {
			  this._port = port;
			  return this;
		 }

		 public virtual OnlineBackupCommandBuilder WithFallbackToFull( bool? flag )
		 {
			  this._fallbackToFull = flag;
			  return this;
		 }

		 public virtual OnlineBackupCommandBuilder WithTimeout( long? timeoutInMillis )
		 {
			  this._timeout = timeoutInMillis;
			  return this;
		 }

		 public virtual OnlineBackupCommandBuilder WithConsistencyCheck( bool? flag )
		 {
			  this._checkConsistency = flag;
			  return this;
		 }

		 public virtual OnlineBackupCommandBuilder WithReportFlag( File consistencyReportLocation )
		 {
			  this._consistencyReportLocation = consistencyReportLocation;
			  return this;
		 }

		 public virtual OnlineBackupCommandBuilder WithAdditionalConfig( Config additionalConfig )
		 {
			  this._additionalConfig = additionalConfig;
			  return this;
		 }

		 public virtual OnlineBackupCommandBuilder WithGraphConsistencyCheck( bool? flag )
		 {
			  this._consistencyCheckGraph = flag;
			  return this;
		 }

		 public virtual OnlineBackupCommandBuilder WithIndexConsistencyCheck( bool? flag )
		 {
			  this._consistencyCheckIndexes = flag;
			  return this;
		 }

		 public virtual OnlineBackupCommandBuilder WithLabelConsistencyCheck( bool? flag )
		 {
			  this._consistencyCheckLabel = flag;
			  return this;
		 }

		 public virtual OnlineBackupCommandBuilder WithOwnerConsistencyCheck( bool? flag )
		 {
			  this._consistencyCheckOwners = flag;
			  return this;
		 }

		 public virtual OnlineBackupCommandBuilder WithOutput( Stream outputStream )
		 {
			  this._output = outputStream;
			  return this;
		 }

		 public virtual OnlineBackupCommandBuilder WithSelectedBackupStrategy( SelectedBackupProtocol selectedBackupStrategy )
		 {
			  this._selectedBackupProtocol = selectedBackupStrategy;
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean backup(java.io.File neo4jHome, String backupName) throws org.neo4j.commandline.admin.CommandFailed, org.neo4j.commandline.admin.IncorrectUsage
		 public virtual bool Backup( File neo4jHome, string backupName )
		 {
			  File targetLocation = new File( neo4jHome, backupName );
			  string[] args;
			  if ( _rawArgs.Present )
			  {
					args = _rawArgs.get();
			  }
			  else
			  {
					try
					{
						 args = ResolveArgs( targetLocation );
					}
					catch ( IOException e )
					{
						 throw new CommandFailed( "Failed to resolve arguments", e );
					}
			  }
			  ( new OnlineBackupCommandProvider() ).Create(neo4jHome.toPath(), ConfigDirFromTarget(neo4jHome.toPath()), ResolveOutsideWorld()).execute(args);
			  return true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String[] resolveArgs(java.io.File targetLocation) throws java.io.IOException
		 public virtual string[] ResolveArgs( File targetLocation )
		 {
			  return Args( ArgBackupName( targetLocation ), ArgBackupLocation( targetLocation ), ArgFrom(), ArgFallbackToFull(), ArgSelectedProtocol(), ArgTimeout(), ArgCheckConsistency(), ArgReportDir(), ArgAdditionalConf(targetLocation), ArgCcGraph(), ArgCcIndexes(), ArgCcLabel(), ArgCcOwners() );
		 }

		 private OutsideWorld ResolveOutsideWorld()
		 {
			  Optional<Stream> output = Optional.ofNullable( this._output );
			  return new ParameterisedOutsideWorld( System.console(), output.orElse(System.out), output.orElse(System.err), System.in, new DefaultFileSystemAbstraction() );
		 }

		 /// <summary>
		 /// Client handles the ports and hosts automatically, so no necessary need to specify </summary>
		 /// <returns> command line parameter for specifying the backup address </returns>
		 private string ArgFrom()
		 {
			  if ( string.ReferenceEquals( _host, null ) && _port == null )
			  {
					return "";
			  }
			  string address = string.join( ":", Optional.ofNullable( _host ).orElse( "" ), Optional.ofNullable( _port ).map( _port => Convert.ToString( _port ) ).orElse( "" ) );
			  return format( "--from=%s", address );
		 }

		 /// <summary>
		 /// The backup location is the directory that stores multiple backups. Each directory in "backup location" is named after the backup name.
		 /// In order for a backup to belong where the user wants it, the backup location is the parent of the target specified by the user. </summary>
		 /// <returns> backup location command line argument </returns>
		 private string ArgBackupLocation( File targetLocation )
		 {
			  string location = Optional.ofNullable( targetLocation ).map( f => targetLocation.ParentFile ).orElseThrow( WrongArguments( "No target location specified" ) ).ToString();
			  return format( "--backup-dir=%s", location );
		 }

		 private string ArgBackupName( File targetLocation )
		 {
			  string backupName = Optional.ofNullable( targetLocation ).map( File.getName ).orElseThrow( WrongArguments( "No target location specified" ) );
			  return format( "--name=%s", backupName );
		 }

		 private static System.Func<System.ArgumentException> WrongArguments( string message )
		 {
			  return () => new System.ArgumentException(message);
		 }

		 private string ArgFallbackToFull()
		 {
			  return Optional.ofNullable( _fallbackToFull ).map( flag => format( "--fallback-to-full=%s", flag ) ).orElse( "" );
		 }

		 private string ArgSelectedProtocol()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return Optional.ofNullable( _selectedBackupProtocol ).map( SelectedBackupProtocol::getName ).map( argValue => format( "--%s=%s", OnlineBackupContextFactory.ARG_NAME_PROTO_OVERRIDE, argValue ) ).orElse( "" );
		 }

		 private string ArgTimeout()
		 {
			  return Optional.ofNullable( this._timeout ).map( value => format( "--timeout=%dms", value ) ).orElse( "" );
		 }

		 private string ArgCcOwners()
		 {
			  return Optional.ofNullable( this._consistencyCheckOwners ).map( value => format( "--check-consistency=%b", this._consistencyCheckOwners ) ).orElse( "" );
		 }

		 private string ArgCcLabel()
		 {
			  return Optional.ofNullable( this._consistencyCheckLabel ).map( value => format( "--cc-label-scan-store=%b", this._consistencyCheckLabel ) ).orElse( "" );
		 }

		 private string ArgCcIndexes()
		 {
			  return Optional.ofNullable( this._consistencyCheckIndexes ).map( value => format( "--cc-indexes=%b", this._consistencyCheckIndexes ) ).orElse( "" );
		 }

		 private string ArgCcGraph()
		 {
			  return Optional.ofNullable( this._consistencyCheckGraph ).map( value => format( "--cc-graph=%b", this._consistencyCheckGraph ) ).orElse( "" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String argAdditionalConf(java.io.File backupTarget) throws java.io.IOException
		 private string ArgAdditionalConf( File backupTarget )
		 {
			  if ( _additionalConfig == null )
			  {
					return "";
			  }
			  File configFile = backupTarget.toPath().resolve("../additional_neo4j.conf").toFile();
			  WriteConfigToFile( _additionalConfig, configFile );

			  return format( "--additional-config=%s", configFile );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void writeConfigToFile(org.neo4j.kernel.configuration.Config config, java.io.File file) throws java.io.IOException
		 internal static void WriteConfigToFile( Config config, File file )
		 {
			  using ( Writer fileWriter = new StreamWriter( file ) )
			  {
					foreach ( KeyValuePair<string, string> entry in config.Raw.SetOfKeyValuePairs() )
					{
						 fileWriter.write( format( "%s=%s\n", entry.Key, entry.Value ) );
					}
			  }
		 }

		 private string ArgReportDir()
		 {
			  return Optional.ofNullable( this._consistencyReportLocation ).map( value => format( "--cc-report-dir=%s", value ) ).orElse( "" );
		 }

		 private string ArgCheckConsistency()
		 {
			  return Optional.ofNullable( this._checkConsistency ).map( value => format( "--check-consistency=%s", value ) ).orElse( "" );
		 }

		 /// <summary>
		 /// Removes empty args and is a convenience method </summary>
		 /// <param name="args"> nullable, can be empty </param>
		 /// <returns> cleaned command line parameters </returns>
		 private static string[] Args( params string[] args )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return Stream.of( args ).filter( StringUtils.isNoneEmpty ).toArray( string[]::new );
		 }

		 private static Path ConfigDirFromTarget( Path neo4jHome )
		 {
			  return neo4jHome.resolve( "conf" );
		 }
	}

}