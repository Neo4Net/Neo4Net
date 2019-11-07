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
namespace Neo4Net.CommandLine.Admin
{

	using Args = Neo4Net.Helpers.Args;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.commandline.Util.Neo4NetVersion;

	public class AdminTool
	{
		 public const int STATUS_SUCCESS = 0;
		 public const int STATUS_ERROR = 1;
		 public static readonly string Neo4NetHome = System.getenv().getOrDefault("Neo4Net_HOME", "");
		 public static readonly string Neo4NetConf = System.getenv().getOrDefault("Neo4Net_CONF", "");
		 public static readonly string Neo4NetDebug = System.getenv().getOrDefault("Neo4Net_DEBUG", null);

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws java.io.IOException
		 public static void Main( string[] args )
		 {
			  Path homeDir = Paths.get( Neo4NetHome );
			  Path configDir = Paths.get( Neo4NetConf );
			  bool debug = !string.ReferenceEquals( Neo4NetDebug, null );

			  using ( RealOutsideWorld outsideWorld = new RealOutsideWorld() )
			  {
					( new AdminTool( CommandLocator.fromServiceLocator(), BlockerLocator.fromServiceLocator(), outsideWorld, debug ) ).Execute(homeDir, configDir, args);
			  }
		 }

		 public const string SCRIPT_NAME = "Neo4Net-admin";
		 private readonly CommandLocator _commandLocator;
		 private readonly BlockerLocator _blockerLocator;
		 private readonly OutsideWorld _outsideWorld;
		 private readonly bool _debug;
		 private readonly Usage _usage;

		 public AdminTool( CommandLocator commandLocator, BlockerLocator blockerLocator, OutsideWorld outsideWorld, bool debug )
		 {
			  this._commandLocator = CommandLocator.withAdditionalCommand( Help(), commandLocator );
			  this._blockerLocator = blockerLocator;
			  this._outsideWorld = outsideWorld;
			  this._debug = debug;
			  this._usage = new Usage( SCRIPT_NAME, this._commandLocator );
		 }

		 public virtual void Execute( Path homeDir, Path configDir, params string[] args )
		 {
			  try
			  {
					if ( args.Length == 0 )
					{
						 BadUsage( "you must provide a command" );
						 return;
					}

					if ( Args.parse( args ).has( "version" ) )
					{
						 _outsideWorld.stdOutLine( "Neo4Net-admin " + Neo4NetVersion() );
						 Success();
						 return;
					}

					string name = args[0];
					string[] commandArgs = Arrays.copyOfRange( args, 1, args.Length );

					AdminCommand_Provider provider;
					try
					{
						 provider = _commandLocator.findProvider( name );
						 foreach ( AdminCommand_Blocker blocker in _blockerLocator.findBlockers( name ) )
						 {
							  if ( blocker.DoesBlock( homeDir, configDir ) )
							  {
									CommandFailed( new CommandFailed( blocker.Explanation() ) );
							  }
						 }
					}
					catch ( NoSuchElementException )
					{
						 BadUsage( format( "unrecognized command: %s", name ) );
						 return;
					}

					if ( provider == null )
					{
						 BadUsage( format( "unrecognized command: %s", name ) );
						 return;
					}

					if ( Args.parse( commandArgs ).has( "help" ) )
					{
						 _outsideWorld.stdErrLine( "unknown argument: --help" );
						 _usage.printUsageForCommand( provider, _outsideWorld.stdErrLine );
						 Failure();
					}
					else
					{
						 AdminCommand command = provider.Create( homeDir, configDir, _outsideWorld );
						 try
						 {
							  command.Execute( commandArgs );
							  Success();
						 }
						 catch ( IncorrectUsage e )
						 {
							  BadUsage( provider, e );
						 }
						 catch ( CommandFailed e )
						 {
							  CommandFailed( e );
						 }
					}
			  }
			  catch ( Exception e )
			  {
					Unexpected( e );
			  }
		 }

		 private System.Func<AdminCommand_Provider> Help()
		 {
			  return () => new HelpCommandProvider(_usage);
		 }

		 private void BadUsage( AdminCommand_Provider command, IncorrectUsage e )
		 {
			  _outsideWorld.stdErrLine( e.Message );
			  _outsideWorld.stdErrLine( "" );
			  _usage.printUsageForCommand( command, _outsideWorld.stdErrLine );
			  Failure();
		 }

		 private void BadUsage( string message )
		 {
			  _outsideWorld.stdErrLine( message );
			  _usage.print( _outsideWorld.stdErrLine );
			  Failure();
		 }

		 private void Unexpected( Exception e )
		 {
			  Failure( "unexpected error", e );
		 }

		 private void CommandFailed( CommandFailed e )
		 {
			  Failure( "command failed", e, e.Code() );
		 }

		 private void Failure()
		 {
			  _outsideWorld.exit( 1 );
		 }

		 private void Failure( string message, Exception e )
		 {
			  Failure( message, e, STATUS_ERROR );
		 }

		 private void Failure( string message, Exception e, int code )
		 {
			  if ( _debug )
			  {
					_outsideWorld.printStacktrace( e );
			  }
			  failure( format( "%s: %s", message, e.Message ), code );
		 }

		 private void Failure( string message, int code )
		 {
			  _outsideWorld.stdErrLine( message );
			  _outsideWorld.exit( code );
		 }

		 private void Success()
		 {
			  _outsideWorld.exit( STATUS_SUCCESS );
		 }
	}

}