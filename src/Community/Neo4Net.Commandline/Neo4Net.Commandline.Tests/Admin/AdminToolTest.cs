using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Commandline.Admin
{
	using Test = org.junit.jupiter.api.Test;
	using InOrder = org.mockito.InOrder;


	using Arguments = Neo4Net.Commandline.Args.Arguments;
	using Iterables = Neo4Net.Helpers.Collection.Iterables;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.commandline.Util.neo4jVersion;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.commandline.admin.AdminTool.STATUS_ERROR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.commandline.admin.AdminTool.STATUS_SUCCESS;

	internal class AdminToolTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldExecuteTheCommand() throws CommandFailed, IncorrectUsage
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldExecuteTheCommand()
		 {
			  AdminCommand command = mock( typeof( AdminCommand ) );
			  ( new AdminTool( CannedCommand( "command", command ), new NullBlockerLocator(), new NullOutsideWorld(), false ) ).execute(null, null, "command", "the", "other", "args");
			  verify( command ).execute( new string[]{ "the", "other", "args" } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldExit0WhenEverythingWorks()
		 internal virtual void ShouldExit0WhenEverythingWorks()
		 {
			  OutsideWorld outsideWorld = mock( typeof( OutsideWorld ) );
			  ( new AdminTool( new CannedLocator( new NullCommandProvider() ), new NullBlockerLocator(), outsideWorld, false ) ).Execute(null, null, "null");
			  verify( outsideWorld ).exit( STATUS_SUCCESS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAddTheHelpCommandToThoseProvidedByTheLocator()
		 internal virtual void ShouldAddTheHelpCommandToThoseProvidedByTheLocator()
		 {
			  OutsideWorld outsideWorld = mock( typeof( OutsideWorld ) );
			  ( new AdminTool( new NullCommandLocator(), new NullBlockerLocator(), outsideWorld, false ) ).Execute(null, null, "help");
			  verify( outsideWorld ).stdOutLine( "    help" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProvideFeedbackWhenNoCommandIsProvided()
		 internal virtual void ShouldProvideFeedbackWhenNoCommandIsProvided()
		 {
			  OutsideWorld outsideWorld = mock( typeof( OutsideWorld ) );
			  ( new AdminTool( new NullCommandLocator(), new NullBlockerLocator(), outsideWorld, false ) ).Execute(null, null);
			  verify( outsideWorld ).stdErrLine( "you must provide a command" );
			  verify( outsideWorld ).stdErrLine( "usage: neo4j-admin <command>" );
			  verify( outsideWorld ).exit( STATUS_ERROR );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProvideFeedbackIfTheCommandThrowsARuntimeException()
		 internal virtual void ShouldProvideFeedbackIfTheCommandThrowsARuntimeException()
		 {
			  OutsideWorld outsideWorld = mock( typeof( OutsideWorld ) );
			  AdminCommand command = args =>
			  {
				throw new Exception( "the-exception-message" );
			  };
			  ( new AdminTool( CannedCommand( "exception", command ), new NullBlockerLocator(), outsideWorld, false ) ).execute(null, null, "exception");
			  verify( outsideWorld ).stdErrLine( "unexpected error: the-exception-message" );
			  verify( outsideWorld ).exit( STATUS_ERROR );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldPrintTheStacktraceWhenTheCommandThrowsARuntimeExceptionIfTheDebugFlagIsSet()
		 internal virtual void ShouldPrintTheStacktraceWhenTheCommandThrowsARuntimeExceptionIfTheDebugFlagIsSet()
		 {
			  OutsideWorld outsideWorld = mock( typeof( OutsideWorld ) );
			  Exception exception = new Exception( "" );
			  AdminCommand command = args =>
			  {
				throw exception;
			  };
			  ( new AdminTool( CannedCommand( "exception", command ), new NullBlockerLocator(), outsideWorld, true ) ).execute(null, null, "exception");
			  verify( outsideWorld ).printStacktrace( exception );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotPrintTheStacktraceWhenTheCommandThrowsARuntimeExceptionIfTheDebugFlagIsNotSet()
		 internal virtual void ShouldNotPrintTheStacktraceWhenTheCommandThrowsARuntimeExceptionIfTheDebugFlagIsNotSet()
		 {
			  OutsideWorld outsideWorld = mock( typeof( OutsideWorld ) );
			  Exception exception = new Exception( "" );
			  AdminCommand command = args =>
			  {
				throw exception;
			  };
			  ( new AdminTool( CannedCommand( "exception", command ), new NullBlockerLocator(), outsideWorld, false ) ).execute(null, null, "exception");
			  verify( outsideWorld, never() ).printStacktrace(exception);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProvideFeedbackIfTheCommandFails()
		 internal virtual void ShouldProvideFeedbackIfTheCommandFails()
		 {
			  OutsideWorld outsideWorld = mock( typeof( OutsideWorld ) );
			  AdminCommand command = args =>
			  {
				throw new CommandFailed( "the-failure-message" );
			  };
			  ( new AdminTool( CannedCommand( "exception", command ), new NullBlockerLocator(), outsideWorld, false ) ).execute(null, null, "exception");
			  verify( outsideWorld ).stdErrLine( "command failed: the-failure-message" );
			  verify( outsideWorld ).exit( STATUS_ERROR );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldPrintTheStacktraceWhenTheCommandFailsIfTheDebugFlagIsSet()
		 internal virtual void ShouldPrintTheStacktraceWhenTheCommandFailsIfTheDebugFlagIsSet()
		 {
			  OutsideWorld outsideWorld = mock( typeof( OutsideWorld ) );
			  CommandFailed exception = new CommandFailed( "" );
			  AdminCommand command = args =>
			  {
				throw exception;
			  };
			  ( new AdminTool( CannedCommand( "exception", command ), new NullBlockerLocator(), outsideWorld, true ) ).execute(null, null, "exception");
			  verify( outsideWorld ).printStacktrace( exception );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotPrintTheStacktraceWhenTheCommandFailsIfTheDebugFlagIsNotSet()
		 internal virtual void ShouldNotPrintTheStacktraceWhenTheCommandFailsIfTheDebugFlagIsNotSet()
		 {
			  OutsideWorld outsideWorld = mock( typeof( OutsideWorld ) );
			  CommandFailed exception = new CommandFailed( "" );
			  AdminCommand command = args =>
			  {
				throw exception;
			  };
			  ( new AdminTool( CannedCommand( "exception", command ), new NullBlockerLocator(), outsideWorld, false ) ).execute(null, null, "exception");
			  verify( outsideWorld, never() ).printStacktrace(exception);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProvideFeedbackIfTheCommandReportsAUsageProblem()
		 internal virtual void ShouldProvideFeedbackIfTheCommandReportsAUsageProblem()
		 {
			  OutsideWorld outsideWorld = mock( typeof( OutsideWorld ) );
			  AdminCommand command = args =>
			  {
				throw new IncorrectUsage( "the-usage-message" );
			  };
			  ( new AdminTool( CannedCommand( "exception", command ), new NullBlockerLocator(), outsideWorld, false ) ).execute(null, null, "exception");
			  InOrder inOrder = inOrder( outsideWorld );
			  inOrder.verify( outsideWorld ).stdErrLine( "the-usage-message" );
			  verify( outsideWorld ).exit( STATUS_ERROR );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBlockDumpIfABlockerSaysSo()
		 internal virtual void ShouldBlockDumpIfABlockerSaysSo()
		 {
			  OutsideWorld outsideWorld = mock( typeof( OutsideWorld ) );
			  AdminCommand command = mock( typeof( AdminCommand ) );

			  AdminCommand_Blocker blocker = mock( typeof( AdminCommand_Blocker ) );
			  when( blocker.DoesBlock( any(), any() ) ).thenReturn(true);
			  when( blocker.Commands() ).thenReturn(Collections.singleton("command"));
			  when( blocker.Explanation() ).thenReturn("the explanation");

			  BlockerLocator blockerLocator = mock( typeof( BlockerLocator ) );
			  when( blockerLocator.FindBlockers( "command" ) ).thenReturn( Collections.singletonList( blocker ) );

			  ( new AdminTool( CannedCommand( "command", command ), blockerLocator, outsideWorld, false ) ).execute( null, null, "command" );

			  verify( outsideWorld ).stdErrLine( "command failed: the explanation" );
			  verify( outsideWorld ).exit( STATUS_ERROR );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBlockDumpIfOneBlockerOutOfManySaysSo()
		 internal virtual void ShouldBlockDumpIfOneBlockerOutOfManySaysSo()
		 {
			  OutsideWorld outsideWorld = mock( typeof( OutsideWorld ) );
			  AdminCommand command = mock( typeof( AdminCommand ) );

			  AdminCommand_Blocker trueBlocker = mock( typeof( AdminCommand_Blocker ) );
			  when( trueBlocker.DoesBlock( any(), any() ) ).thenReturn(true);
			  when( trueBlocker.Explanation() ).thenReturn("trueBlocker explanation");

			  AdminCommand_Blocker falseBlocker = mock( typeof( AdminCommand_Blocker ) );
			  when( falseBlocker.DoesBlock( any(), any() ) ).thenReturn(false);
			  when( falseBlocker.Explanation() ).thenReturn("falseBlocker explanation");

			  BlockerLocator blockerLocator = mock( typeof( BlockerLocator ) );
			  when( blockerLocator.FindBlockers( "command" ) ).thenReturn( Arrays.asList( falseBlocker, trueBlocker, falseBlocker ) );

			  ( new AdminTool( CannedCommand( "command", command ), blockerLocator, outsideWorld, false ) ).execute( null, null, "command" );

			  verify( outsideWorld ).stdErrLine( "command failed: trueBlocker explanation" );
			  verify( outsideWorld ).exit( STATUS_ERROR );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotBlockIfNoneOfTheBlockersBlock() throws CommandFailed, IncorrectUsage
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldNotBlockIfNoneOfTheBlockersBlock()
		 {
			  AdminCommand command = mock( typeof( AdminCommand ) );

			  AdminCommand_Blocker falseBlocker = mock( typeof( AdminCommand_Blocker ) );
			  when( falseBlocker.DoesBlock( any(), any() ) ).thenReturn(false);
			  when( falseBlocker.Explanation() ).thenReturn("falseBlocker explanation");

			  BlockerLocator blockerLocator = mock( typeof( BlockerLocator ) );
			  when( blockerLocator.FindBlockers( "command" ) ).thenReturn( Arrays.asList( falseBlocker, falseBlocker, falseBlocker ) );

			  ( new AdminTool( CannedCommand( "command", command ), blockerLocator, new NullOutsideWorld(), false ) ).execute(null, null, "command", "the", "other", "args");
			  verify( command ).execute( new string[]{ "the", "other", "args" } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void helpArgumentPrintsHelp()
		 internal virtual void HelpArgumentPrintsHelp()
		 {
			  AdminCommand command = mock( typeof( AdminCommand ) );
			  OutsideWorld outsideWorld = mock( typeof( OutsideWorld ) );

			  ( new AdminTool( CannedCommand( "command", command ), new NullBlockerLocator(), outsideWorld, false ) ).execute(null, null, "--help");

			  verifyNoMoreInteractions( command );
			  verify( outsideWorld ).stdErrLine( "unrecognized command: --help" );
			  verify( outsideWorld ).stdErrLine( "usage: neo4j-admin <command>" );
			  verify( outsideWorld ).exit( STATUS_ERROR );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void helpArgumentPrintsHelpForCommand()
		 internal virtual void HelpArgumentPrintsHelpForCommand()
		 {
			  AdminCommand command = mock( typeof( AdminCommand ) );
			  OutsideWorld outsideWorld = mock( typeof( OutsideWorld ) );

			  ( new AdminTool( CannedCommand( "command", command ), new NullBlockerLocator(), outsideWorld, false ) ).execute(null, null, "command", "--help");

			  verifyNoMoreInteractions( command );
			  verify( outsideWorld ).stdErrLine( "unknown argument: --help" );
			  verify( outsideWorld ).stdErrLine( "usage: neo4j-admin command " );
			  verify( outsideWorld ).exit( STATUS_ERROR );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void versionArgumentPrintsVersion()
		 internal virtual void VersionArgumentPrintsVersion()
		 {
			  AdminCommand command = mock( typeof( AdminCommand ) );
			  OutsideWorld outsideWorld = mock( typeof( OutsideWorld ) );

			  ( new AdminTool( CannedCommand( "command", command ), new NullBlockerLocator(), outsideWorld, false ) ).execute(null, null, "--version");

			  verifyNoMoreInteractions( command );
			  verify( outsideWorld ).stdOutLine( "neo4j-admin " + neo4jVersion() );
			  verify( outsideWorld ).exit( STATUS_SUCCESS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void versionArgumentPrintsVersionEvenWithCommand()
		 internal virtual void VersionArgumentPrintsVersionEvenWithCommand()
		 {
			  AdminCommand command = mock( typeof( AdminCommand ) );
			  OutsideWorld outsideWorld = mock( typeof( OutsideWorld ) );

			  ( new AdminTool( CannedCommand( "command", command ), new NullBlockerLocator(), outsideWorld, false ) ).execute(null, null, "command", "--version");

			  verifyNoMoreInteractions( command );
			  verify( outsideWorld ).stdOutLine( "neo4j-admin " + neo4jVersion() );
			  verify( outsideWorld ).exit( STATUS_SUCCESS );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static CannedLocator cannedCommand(final String name, AdminCommand command)
		 private static CannedLocator CannedCommand( string name, AdminCommand command )
		 {
			  return new CannedLocator( new AdminCommand_ProviderAnonymousInnerClass( name, command ) );
		 }

		 private class AdminCommand_ProviderAnonymousInnerClass : AdminCommand_Provider
		 {
			 private Neo4Net.Commandline.Admin.AdminCommand _command;

			 public AdminCommand_ProviderAnonymousInnerClass( string name, Neo4Net.Commandline.Admin.AdminCommand command ) : base( name )
			 {
				 this._command = command;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public org.neo4j.commandline.arguments.Arguments allArguments()
			 public override Arguments allArguments()
			 {
				  return Arguments.NO_ARGS;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public String description()
			 public override string description()
			 {
				  return "";
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public String summary()
			 public override string summary()
			 {
				  return "";
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public AdminCommandSection commandSection()
			 public override AdminCommandSection commandSection()
			 {
				  return AdminCommandSection.General();
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public AdminCommand create(java.nio.file.Path homeDir, java.nio.file.Path configDir, OutsideWorld outsideWorld)
			 public override AdminCommand create( Path homeDir, Path configDir, OutsideWorld outsideWorld )
			 {
				  return _command;
			 }
		 }

		 private class NullCommandLocator : CommandLocator
		 {
			  public override AdminCommand_Provider FindProvider( string s )
			  {
					throw new System.NotSupportedException( "not implemented" );
			  }

			  public virtual IEnumerable<AdminCommand_Provider> AllProviders
			  {
				  get
				  {
						return Iterables.empty();
				  }
			  }
		 }

		 private class NullCommandProvider : AdminCommand_Provider
		 {
			  internal NullCommandProvider() : base("null")
			  {
			  }

			  public override Arguments AllArguments()
			  {
					return Arguments.NO_ARGS;
			  }

			  public override string Description()
			  {
					return "";
			  }

			  public override string Summary()
			  {
					return "";
			  }

			  public override AdminCommandSection CommandSection()
			  {
					return AdminCommandSection.General();
			  }

			  public override AdminCommand Create( Path homeDir, Path configDir, OutsideWorld outsideWorld )
			  {
					return args =>
					{
					};
			  }
		 }

		 private class NullBlockerLocator : BlockerLocator
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Iterable<AdminCommand_Blocker> findBlockers(String name) throws java.util.NoSuchElementException
			  public override IEnumerable<AdminCommand_Blocker> FindBlockers( string name )
			  {
					return Collections.emptyList();
			  }
		 }
	}

}