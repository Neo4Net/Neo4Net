using System;
using System.Collections.Generic;

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
namespace Neo4Net.Server.plugins
{

	using Service = Neo4Net.Helpers.Service;

	/// <summary>
	/// API for creating extensions for the Neo4Net server.
	/// <para>
	/// Extensions are created by creating a subclass of this class. The subclass
	/// should have a public no-argument constructor (or no constructor at all).
	/// Then place this class in a jar-file that contains a file called
	/// <code>META-INF/services/org.Neo4Net.server.plugins.ServerPlugin</code>.
	/// This file should contain the fully qualified name of the class that extends
	/// <seealso cref="ServerPlugin"/>, e.g. <code>com.example.MyNeo4NetServerExtension</code>
	/// on a single line. If the jar contains multiple extensions to the Neo4Net
	/// server, this file should contain the fully qualified class names of all
	/// extension classes, each class name on its own line in the file. When the jar
	/// file is placed on the class path of the server, it will be loaded
	/// automatically when the server starts.
	/// </para>
	/// <para>
	/// The easiest way to implement Neo4Net server extensions is by defining public
	/// methods on the extension class annotated with
	/// <code>@<seealso cref="PluginTarget"/></code>. The parameter for the
	/// <seealso cref="PluginTarget"/> annotation should be the class representing the
	/// IEntity that the method extends. The entities that can be extended are
	/// currently:
	/// <ul>
	/// <li><seealso cref="org.Neo4Net.graphdb.GraphDatabaseService"/> - a general extension
	/// method to the server</li>
	/// <li><seealso cref="org.Neo4Net.graphdb.Node"/> - an extension method for a node</li>
	/// <li><seealso cref="org.Neo4Net.graphdb.Relationship"/> - an extension method for a
	/// relationship</li>
	/// </ul>
	/// You can then use the <code>@<seealso cref="Source"/></code> annotation on one parameter of the method to get
	/// the instance that was extended. Additional parameters needs to be of a
	/// supported type, and annotated with the <code>@<seealso cref="Parameter"/></code> annotation specifying the name by which the
	/// parameter is passed from the client. The supported parameter types are:
	/// <ul>
	/// <li><seealso cref="org.Neo4Net.graphdb.Node"/></li>
	/// <li><seealso cref="org.Neo4Net.graphdb.Relationship"/></li>
	/// <li><seealso cref="java.net.URI"/> or <seealso cref="java.net.URL"/></li>
	/// <li><seealso cref="Integer"/></li>
	/// <li><seealso cref="Long"/></li>
	/// <li><seealso cref="Short"/></li>
	/// <li><seealso cref="Byte"/></li>
	/// <li><seealso cref="Character"/></li>
	/// <li><seealso cref="Boolean"/></li>
	/// <li><seealso cref="Double"/></li>
	/// <li><seealso cref="Float"/></li>
	/// <li><seealso cref="System.Collections.IList lists"/>, <seealso cref="System.Collections.Generic.ISet<object> sets"/> or arrays of any
	/// of the above types.</li>
	/// </ul>
	/// </para>
	/// <para>
	/// All exceptions thrown by an <seealso cref="PluginTarget"/> method are treated as
	/// server errors, unless the method has declared the ability to throw such an
	/// exception, in that case the exception is treated as a bad request and
	/// propagated to the invoking client.
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= java.util.ServiceLoader </seealso>
	/// @deprecated Server plugins are deprecated for removal in the next major release. Please use unmanaged extensions instead. 
	[Obsolete("Server plugins are deprecated for removal in the next major release. Please use unmanaged extensions instead.")]
	public abstract class ServerPlugin
	{
		 internal readonly string Name;

		 /// <summary>
		 /// Create a server extension with the specified name.
		 /// </summary>
		 /// <param name="name"> the name of this extension. </param>
		 [Obsolete]
		 public ServerPlugin( string name )
		 {
			  this.Name = VerifyName( name );
		 }

		 /// <summary>
		 /// Create a server extension using the simple name of the concrete class
		 /// that extends <seealso cref="ServerPlugin"/> as the name for the extension.
		 /// </summary>
		 [Obsolete]
		 public ServerPlugin()
		 {
			  this.Name = VerifyName( this.GetType().Name );
		 }

		 internal static string VerifyName( string name )
		 {
			  if ( string.ReferenceEquals( name, null ) )
			  {
					throw new System.ArgumentException( "Name may not be null" );
			  }
			  try
			  {
					if ( !URLEncoder.encode( name, StandardCharsets.UTF_8.name() ).Equals(name) )
					{
						 throw new System.ArgumentException( "Name contains illegal characters" );
					}
			  }
			  catch ( UnsupportedEncodingException e )
			  {
					throw new Exception( "UTF-8 should be supported", e );
			  }
			  return name;
		 }

		 public override string ToString()
		 {
			  return "ServerPlugin[" + Name + "]";
		 }

		 internal static IEnumerable<ServerPlugin> Load()
		 {
			  return Service.load( typeof( ServerPlugin ) );
		 }

		 /// <summary>
		 /// Loads the extension points of this server extension. Override this method
		 /// to provide your own, custom way of loading extension points. The default
		 /// implementation loads <seealso cref="PluginPoint"/> based on methods with the
		 /// <seealso cref="PluginTarget"/> annotation.
		 /// </summary>
		 /// <param name="extender">     the collection of <seealso cref="org.Neo4Net.server.plugins.PluginPoint"/>s for this
		 ///                     <seealso cref="org.Neo4Net.server.plugins.ServerPlugin"/>.
		 ///  </param>
		 [Obsolete]
		 protected internal virtual void LoadServerExtender( ServerExtender extender )
		 {
			  foreach ( PluginPoint plugin in GetDefaultExtensionPoints( extender.PluginPointFactory ) )
			  {
					extender.AddExtension( plugin.ForType(), plugin );
			  }
		 }

		 /// <summary>
		 /// Loads the extension points of this server extension. Override this method
		 /// to provide your own, custom way of loading extension points. The default
		 /// implementation loads <seealso cref="PluginPoint"/> based on methods with the
		 /// <seealso cref="PluginTarget"/> annotation.
		 /// </summary>
		 /// <returns> the collection of <seealso cref="PluginPoint"/>s for this
		 ///         <seealso cref="ServerPlugin"/>. </returns>
		 [Obsolete]
		 protected internal virtual ICollection<PluginPoint> GetDefaultExtensionPoints( PluginPointFactory pluginPointFactory )
		 {
			  IList<PluginPoint> result = new List<PluginPoint>();
			  foreach ( System.Reflection.MethodInfo method in this.GetType().GetMethods() )
			  {
					PluginTarget target = method.getAnnotation( typeof( PluginTarget ) );
					if ( target != null )
					{
						 result.Add( pluginPointFactory.CreateFrom( this, method, target.value() ) );
					}
			  }
			  return result;
		 }
	}

}