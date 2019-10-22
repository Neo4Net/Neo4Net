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
namespace Neo4Net.causalclustering.protocol.handshake
{

	using Neo4Net.causalclustering.protocol;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
	using static Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocol;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
	using static Neo4Net.causalclustering.protocol.Protocol_ModifierProtocol;

	public class ProtocolStack
	{
		 private readonly ApplicationProtocol _applicationProtocol;
		 private readonly IList<ModifierProtocol> _modifierProtocols;

		 public ProtocolStack( ApplicationProtocol applicationProtocol, IList<ModifierProtocol> modifierProtocols )
		 {
			  this._applicationProtocol = applicationProtocol;
			  this._modifierProtocols = Collections.unmodifiableList( modifierProtocols );
		 }

		 public virtual ApplicationProtocol ApplicationProtocol()
		 {
			  return _applicationProtocol;
		 }

		 public virtual IList<ModifierProtocol> ModifierProtocols()
		 {
			  return _modifierProtocols;
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }
			  ProtocolStack that = ( ProtocolStack ) o;
			  return Objects.Equals( _applicationProtocol, that._applicationProtocol ) && Objects.Equals( _modifierProtocols, that._modifierProtocols );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _applicationProtocol, _modifierProtocols );
		 }

		 public override string ToString()
		 {
			  string desc = format( "%s version:%d", _applicationProtocol.category(), _applicationProtocol.implementation() );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IList<string> modifierNames = _modifierProtocols.Select( Protocol::implementation ).ToList();

			  if ( modifierNames.Count > 0 )
			  {
					desc = format( "%s (%s)", desc, join( ", ", modifierNames ) );
			  }

			  return desc;
		 }

		 public static Builder Builder()
		 {
			  return new Builder();
		 }

		 public class Builder
		 {
			  internal ApplicationProtocol ApplicationProtocol;
			  internal readonly IList<ModifierProtocol> ModifierProtocols = new List<ModifierProtocol>();

			  internal Builder()
			  {
			  }

			  public virtual Builder Modifier( ModifierProtocol modifierProtocol )
			  {
					ModifierProtocols.Add( modifierProtocol );
					return this;
			  }

			  public virtual Builder Application( ApplicationProtocol applicationProtocol )
			  {
					this.ApplicationProtocol = applicationProtocol;
					return this;
			  }

			  internal virtual ProtocolStack Build()
			  {
					return new ProtocolStack( ApplicationProtocol, ModifierProtocols );
			  }

			  public override string ToString()
			  {
					return "Builder{" + "applicationProtocol=" + ApplicationProtocol + ", modifierProtocols=" + ModifierProtocols + '}';
			  }
		 }
	}

}