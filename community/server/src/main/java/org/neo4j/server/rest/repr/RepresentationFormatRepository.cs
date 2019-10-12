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
namespace Org.Neo4j.Server.rest.repr
{

	using Service = Org.Neo4j.Helpers.Service;
	using PluginManager = Org.Neo4j.Server.plugins.PluginManager;
	using JsonFormat = Org.Neo4j.Server.rest.repr.formats.JsonFormat;

	public sealed class RepresentationFormatRepository
	{
		 private readonly IDictionary<MediaType, RepresentationFormat> _formats;
		 private readonly AbstractNeoServer _injectorProvider;

		 public RepresentationFormatRepository( AbstractNeoServer injectorProvider )
		 {
			  this._injectorProvider = injectorProvider;
			  this._formats = new Dictionary<MediaType, RepresentationFormat>();
			  foreach ( RepresentationFormat format in Service.load( typeof( RepresentationFormat ) ) )
			  {
					_formats[format.MediaType] = format;
			  }
		 }

		 public OutputFormat OutputFormat( IList<MediaType> acceptable, URI baseUri, MultivaluedMap<string, string> requestHeaders )
		 {
			  RepresentationFormat format = ForHeaders( acceptable, requestHeaders );
			  if ( format == null )
			  {
					format = ForMediaTypes( acceptable );
			  }
			  if ( format == null )
			  {
					format = UseDefault( acceptable );
			  }
			  return new OutputFormat( format, baseUri, ExtensionManager );
		 }

		 private PluginManager ExtensionManager
		 {
			 get
			 {
				  return _injectorProvider == null ? null : _injectorProvider.ExtensionManager;
			 }
		 }

		 private RepresentationFormat ForHeaders( IList<MediaType> acceptable, MultivaluedMap<string, string> requestHeaders )
		 {
			  if ( requestHeaders == null )
			  {
					return null;
			  }
			  if ( !ContainsType( acceptable, MediaType.APPLICATION_JSON_TYPE ) )
			  {
					return null;
			  }
			  string streamHeader = requestHeaders.getFirst( StreamingFormat_Fields.STREAM_HEADER );
			  if ( "true".Equals( streamHeader, StringComparison.OrdinalIgnoreCase ) )
			  {
					return _formats[StreamingFormat_Fields.MediaType];
			  }
			  return null;
		 }

		 private bool ContainsType( IList<MediaType> mediaTypes, MediaType mediaType )
		 {
			  foreach ( MediaType type in mediaTypes )
			  {
					if ( mediaType.Type.Equals( type.Type ) && mediaType.Subtype.Equals( type.Subtype ) )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 private RepresentationFormat ForMediaTypes( IList<MediaType> acceptable )
		 {
			  foreach ( MediaType type in acceptable )
			  {
					RepresentationFormat format = _formats[type];
					if ( format != null )
					{
						 return format;
					}
			  }
			  return null;
		 }

		 public InputFormat InputFormat( MediaType type )
		 {
			  if ( type == null )
			  {
					return UseDefault();
			  }

			  RepresentationFormat format = _formats[type];
			  if ( format != null )
			  {
					return format;
			  }

			  format = _formats[new MediaType( type.Type, type.Subtype )];
			  if ( format != null )
			  {
					return format;
			  }

			  return UseDefault( type );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private DefaultFormat useDefault(final java.util.List<javax.ws.rs.core.MediaType> acceptable)
		 private DefaultFormat UseDefault( IList<MediaType> acceptable )
		 {
			  return UseDefault( acceptable.ToArray() );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private DefaultFormat useDefault(final javax.ws.rs.core.MediaType... type)
		 private DefaultFormat UseDefault( params MediaType[] type )
		 {
			  return new DefaultFormat( new JsonFormat(), _formats.Keys, type );
		 }
	}

}