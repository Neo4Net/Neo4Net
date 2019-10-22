using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.logging
{

	using RaftMessages = Neo4Net.causalclustering.core.consensus.RaftMessages;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;


	public class BetterMessageLogger<MEMBER> : LifecycleAdapter, MessageLogger<MEMBER>
	{
		 private sealed class Direction
		 {
			  public static readonly Direction Info = new Direction( "Info", InnerEnum.Info, "---" );
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           OUTBOUND("-->"),
			  public static readonly Direction Inbound = new Direction( "Inbound", InnerEnum.Inbound, "<--" );

			  private static readonly IList<Direction> valueList = new List<Direction>();

			  static Direction()
			  {
				  valueList.Add( Info );
				  valueList.Add( OUTBOUND );
				  valueList.Add( Inbound );
			  }

			  public enum InnerEnum
			  {
				  Info,
				  OUTBOUND,
				  Inbound
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal Public readonly;

			  internal Direction( string name, InnerEnum innerEnum, string arrow )
			  {
					this.Arrow = arrow;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			 public static IList<Direction> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static Direction valueOf( string name )
			 {
				 foreach ( Direction enumInstance in Direction.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 private readonly PrintWriter _printWriter;
		 private readonly Clock _clock;
		 private readonly DateTimeFormatter _dateTimeFormatter = DateTimeFormatter.ofPattern( "yyyy-MM-dd HH:mm:ss.SSSZ" );

		 public BetterMessageLogger( MEMBER myself, PrintWriter printWriter, Clock clock )
		 {
			  this._printWriter = printWriter;
			  this._clock = clock;
			  Log( myself, Direction.Info, myself, "Info", "I am " + myself );
		 }

		 private void Log( MEMBER me, Direction direction, MEMBER remote, string type, string message )
		 {
			  _printWriter.println( format( "%s %s %s %s %s \"%s\"", ZonedDateTime.now( _clock ).format( _dateTimeFormatter ), me, direction.arrow, remote, type, message ) );
			  _printWriter.flush();
		 }

		 public override void LogOutbound<M>( MEMBER me, M message, MEMBER remote ) where M : Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage
		 {
			  Log( me, Direction.Outbound, remote, NullSafeMessageType( message ), valueOf( message ) );
		 }

		 public override void LogInbound<M>( MEMBER remote, M message, MEMBER me ) where M : Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage
		 {
			  Log( me, Direction.Inbound, remote, NullSafeMessageType( message ), valueOf( message ) );
		 }

		 private string NullSafeMessageType<M>( M message ) where M : Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage
		 {
			  if ( Objects.isNull( message ) )
			  {
					return "null";
			  }
			  else
			  {
					return message.type().ToString();
			  }
		 }

		 public override void Stop()
		 {
			  _printWriter.close();
		 }
	}

}