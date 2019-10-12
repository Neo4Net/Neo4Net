using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.protocol
{
	using Channel = io.netty.channel.Channel;


	public interface ProtocolInstaller<O> where O : ProtocolInstaller_Orientation
	{

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void install(io.netty.channel.Channel channel) throws Exception;
		 void Install( Channel channel );

		 /// <summary>
		 /// For testing
		 /// </summary>
		 Protocol_ApplicationProtocol ApplicationProtocol();

		 /// <summary>
		 /// For testing
		 /// </summary>
		 ICollection<ICollection<Protocol_ModifierProtocol>> Modifiers();
	}

	 public abstract class ProtocolInstaller_Factory<O, I> where O : ProtocolInstaller_Orientation where I : ProtocolInstaller<O>
	 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly Protocol_ApplicationProtocol ApplicationProtocolConflict;
		  internal readonly System.Func<IList<ModifierProtocolInstaller<O>>, I> Constructor;

		  protected internal ProtocolInstaller_Factory( Protocol_ApplicationProtocol applicationProtocol, System.Func<IList<ModifierProtocolInstaller<O>>, I> constructor )
		  {
				this.ApplicationProtocolConflict = applicationProtocol;
				this.Constructor = constructor;
		  }

		  internal virtual I Create( IList<ModifierProtocolInstaller<O>> modifiers )
		  {
				return Constructor.apply( modifiers );
		  }

		  public virtual Protocol_ApplicationProtocol ApplicationProtocol()
		  {
				return ApplicationProtocolConflict;
		  }
	 }

	 public interface ProtocolInstaller_Orientation
	 {
	 }

	  public interface ProtocolInstaller_Orientation_Server : ProtocolInstaller_Orientation
	  {
	  }

	  public static class ProtocolInstaller_Orientation_Server_Fields
	  {
			public const string INBOUND = "inbound";
	  }

	  public interface ProtocolInstaller_Orientation_Client : ProtocolInstaller_Orientation
	  {
	  }

	  public static class ProtocolInstaller_Orientation_Client_Fields
	  {
			public const string OUTBOUND = "outbound";
	  }

}