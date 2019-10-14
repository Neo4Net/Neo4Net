using System;
using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Server.rest.repr
{

	internal abstract class Serializer
	{
		 private readonly URI _baseUri;
		 private readonly ExtensionInjector _extensions;

		 internal Serializer( URI baseUri, ExtensionInjector extensions )
		 {
			  this._baseUri = baseUri;
			  this._extensions = extensions;
		 }

		 internal void Serialize( MappingWriter mapping, MappingRepresentation value )
		 {
			  InjectExtensions( mapping, value, _baseUri, _extensions );
			  value.Serialize( new MappingSerializer( mapping, _baseUri, _extensions ) );
			  mapping.Done();
		 }

		 internal static void InjectExtensions( MappingWriter mapping, MappingRepresentation value, URI baseUri, ExtensionInjector injector )
		 {
			  if ( value is ExtensibleRepresentation && injector != null )
			  {
					IDictionary<string, IList<string>> extData = injector.GetExensionsFor( value.Type.extend );
					string entityIdentity = ( ( ExtensibleRepresentation ) value ).Identity;
					if ( extData != null )
					{
						 MappingWriter extensions = mapping.NewMapping( RepresentationType.Plugins, "extensions" );
						 foreach ( KeyValuePair<string, IList<string>> ext in extData.SetOfKeyValuePairs() )
						 {
							  MappingWriter extension = extensions.newMapping( RepresentationType.Plugin, ext.Key );
							  foreach ( string method in ext.Value )
							  {
									StringBuilder path = ( new StringBuilder( "/ext/" ) ).Append( ext.Key );
									path.Append( "/" ).Append( value.Type.valueName );
									if ( !string.ReferenceEquals( entityIdentity, null ) )
									{
										 path.Append( "/" ).Append( entityIdentity );
									}
									path.Append( "/" ).Append( method );
									extension.writeValue( RepresentationType.Uri, method, JoinBaseWithRelativePath( baseUri, path.ToString() ) );
							  }
							  extension.Done();
						 }
						 extensions.Done();
					}
			  }
		 }

		 internal void Serialize( ListWriter list, ListRepresentation value )
		 {
			  value.Serialize( new ListSerializer( list, _baseUri, _extensions ) );
			  list.Done();
		 }

		 internal string RelativeUri( string path )
		 {
			  return JoinBaseWithRelativePath( _baseUri, path );
		 }

		 internal string RelativeTemplate( string path )
		 {
			  return JoinBaseWithRelativePath( _baseUri, path );
		 }

		 internal static string JoinBaseWithRelativePath( URI baseUri, string path )
		 {
			  string @base = baseUri.ToString();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final StringBuilder result = new StringBuilder(super.length() + path.length() + 1).append(super);
			  StringBuilder result = ( new StringBuilder( @base.Length + path.Length + 1 ) ).Append( @base );
			  if ( @base.EndsWith( "/", StringComparison.Ordinal ) )
			  {
					if ( path.StartsWith( "/", StringComparison.Ordinal ) )
					{
						 return result.Append( path.Substring( 1 ) ).ToString();
					}
			  }
			  else if ( !path.StartsWith( "/", StringComparison.Ordinal ) )
			  {
					return result.Append( '/' ).Append( path ).ToString();
			  }
			  return result.Append( path ).ToString();
		 }

		 internal virtual void CheckThatItIsBuiltInType( object value )
		 {
			  if ( !"java.lang".Equals( value.GetType().Assembly.GetName().Name ) )
			  {
					throw new System.ArgumentException( "Unsupported number type: " + value.GetType() );
			  }
		 }
	}

}