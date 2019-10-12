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
namespace Org.Neo4j.causalclustering.protocol
{

	using ProtocolStack = Org.Neo4j.causalclustering.protocol.handshake.ProtocolStack;

	public class ProtocolInstallerRepository<O> where O : ProtocolInstaller_Orientation
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Map<org.neo4j.causalclustering.protocol.Protocol_ApplicationProtocol,ProtocolInstaller_Factory<O,?>> installers;
		 private readonly IDictionary<Protocol_ApplicationProtocol, ProtocolInstaller_Factory<O, ?>> _installers;
		 private readonly IDictionary<Protocol_ModifierProtocol, ModifierProtocolInstaller<O>> _modifiers;

		 public ProtocolInstallerRepository<T1>( ICollection<T1> installers, ICollection<ModifierProtocolInstaller<O>> modifiers )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<org.neo4j.causalclustering.protocol.Protocol_ApplicationProtocol,ProtocolInstaller_Factory<O,?>> tempInstallers = new java.util.HashMap<>();
			  IDictionary<Protocol_ApplicationProtocol, ProtocolInstaller_Factory<O, ?>> tempInstallers = new Dictionary<Protocol_ApplicationProtocol, ProtocolInstaller_Factory<O, ?>>();
			  installers.forEach( installer => addTo( tempInstallers, installer, installer.applicationProtocol() ) );
			  this._installers = unmodifiableMap( tempInstallers );

			  IDictionary<Protocol_ModifierProtocol, ModifierProtocolInstaller<O>> tempModifierInstallers = new Dictionary<Protocol_ModifierProtocol, ModifierProtocolInstaller<O>>();
			  modifiers.forEach( installer => installer.protocols().forEach(protocol => addTo(tempModifierInstallers, installer, protocol)) );
			  this._modifiers = unmodifiableMap( tempModifierInstallers );
		 }

		 private void AddTo<T, P>( IDictionary<P, T> tempServerMap, T installer, P protocol ) where P : Protocol
		 {
			  T old = tempServerMap[protocol] = installer;
			  if ( old != default( T ) )
			  {
					throw new System.ArgumentException( string.Format( "Duplicate protocol installers for protocol {0}: {1} and {2}", protocol, installer, old ) );
			  }
		 }

		 public virtual ProtocolInstaller<O> InstallerFor( ProtocolStack protocolStack )
		 {
			  Protocol_ApplicationProtocol applicationProtocol = protocolStack.ApplicationProtocol();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ProtocolInstaller_Factory<O,?> protocolInstaller = installers.get(applicationProtocol);
			  ProtocolInstaller_Factory<O, ?> protocolInstaller = _installers[applicationProtocol];

			  EnsureKnownProtocol( applicationProtocol, protocolInstaller );

			  return protocolInstaller.Create( GetModifierProtocolInstallers( protocolStack ) );
		 }

		 private IList<ModifierProtocolInstaller<O>> GetModifierProtocolInstallers( ProtocolStack protocolStack )
		 {
			  IList<ModifierProtocolInstaller<O>> modifierProtocolInstallers = new List<ModifierProtocolInstaller<O>>();
			  foreach ( Protocol_ModifierProtocol modifierProtocol in protocolStack.ModifierProtocols() )
			  {
					EnsureNotDuplicate( modifierProtocolInstallers, modifierProtocol );

					ModifierProtocolInstaller<O> protocolInstaller = _modifiers[modifierProtocol];

					EnsureKnownProtocol( modifierProtocol, protocolInstaller );

					modifierProtocolInstallers.Add( protocolInstaller );
			  }
			  return modifierProtocolInstallers;
		 }

		 private void EnsureNotDuplicate( IList<ModifierProtocolInstaller<O>> modifierProtocolInstallers, Protocol_ModifierProtocol modifierProtocol )
		 {
			  bool duplicateIdentifier = modifierProtocolInstallers.stream().flatMap(modifier => modifier.protocols().stream()).anyMatch(protocol => protocol.category().Equals(modifierProtocol.category()));
			  if ( duplicateIdentifier )
			  {
					throw new System.ArgumentException( "Attempted to install multiple versions of " + modifierProtocol.category() );
			  }
		 }

		 private void EnsureKnownProtocol( Protocol protocol, object protocolInstaller )
		 {
			  if ( protocolInstaller == null )
			  {
					throw new System.InvalidOperationException( string.Format( "Installer for requested protocol {0} does not exist", protocol ) );
			  }
		 }
	}

}