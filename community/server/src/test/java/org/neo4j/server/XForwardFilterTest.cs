using System;

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
namespace Org.Neo4j.Server
{
	using ContainerException = com.sun.jersey.api.container.ContainerException;
	using HttpContext = com.sun.jersey.api.core.HttpContext;
	using ResourceConfig = com.sun.jersey.api.core.ResourceConfig;
	using ResourceContext = com.sun.jersey.api.core.ResourceContext;
	using InBoundHeaders = com.sun.jersey.core.header.InBoundHeaders;
	using IoCComponentProviderFactory = com.sun.jersey.core.spi.component.ioc.IoCComponentProviderFactory;
	using FeaturesAndProperties = com.sun.jersey.core.util.FeaturesAndProperties;
	using ServerInjectableProviderFactory = com.sun.jersey.server.impl.inject.ServerInjectableProviderFactory;
	using MessageBodyWorkers = com.sun.jersey.spi.MessageBodyWorkers;
	using ContainerRequest = com.sun.jersey.spi.container.ContainerRequest;
	using ContainerResponse = com.sun.jersey.spi.container.ContainerResponse;
	using ContainerResponseWriter = com.sun.jersey.spi.container.ContainerResponseWriter;
	using ExceptionMapperContext = com.sun.jersey.spi.container.ExceptionMapperContext;
	using WebApplication = com.sun.jersey.spi.container.WebApplication;
	using DispatchingListener = com.sun.jersey.spi.monitoring.DispatchingListener;
	using RequestListener = com.sun.jersey.spi.monitoring.RequestListener;
	using ResponseListener = com.sun.jersey.spi.monitoring.ResponseListener;
	using Test = org.junit.Test;


	using XForwardFilter = Org.Neo4j.Server.web.XForwardFilter;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class XForwardFilterTest
	{
		 private const string X_FORWARD_HOST_HEADER_KEY = "X-Forwarded-Host";
		 private const string X_FORWARD_PROTO_HEADER_KEY = "X-Forwarded-Proto";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetTheBaseUriToTheSameValueAsTheXForwardHostHeader()
		 public virtual void ShouldSetTheBaseUriToTheSameValueAsTheXForwardHostHeader()
		 {
			  // given
			  const string xForwardHostAndPort = "jimwebber.org:1234";

			  XForwardFilter filter = new XForwardFilter();

			  InBoundHeaders headers = new InBoundHeaders();
			  headers.add( X_FORWARD_HOST_HEADER_KEY, xForwardHostAndPort );

			  ContainerRequest request = new ContainerRequest( WEB_APPLICATION, "GET", URI.create( "http://iansrobinson.com" ), URI.create( "http://iansrobinson.com/foo/bar" ), headers, INPUT_STREAM );

			  // when
			  ContainerRequest result = filter.Filter( request );

			  // then
			  assertThat( result.BaseUri.ToString(), containsString(xForwardHostAndPort) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetTheRequestUriToTheSameValueAsTheXForwardHostHeader()
		 public virtual void ShouldSetTheRequestUriToTheSameValueAsTheXForwardHostHeader()
		 {
			  // given
			  const string xForwardHostAndPort = "jimwebber.org:1234";

			  XForwardFilter filter = new XForwardFilter();

			  InBoundHeaders headers = new InBoundHeaders();
			  headers.add( X_FORWARD_HOST_HEADER_KEY, xForwardHostAndPort );

			  ContainerRequest request = new ContainerRequest( WEB_APPLICATION, "GET", URI.create( "http://iansrobinson.com" ), URI.create( "http://iansrobinson.com/foo/bar" ), headers, INPUT_STREAM );

			  // when
			  ContainerRequest result = filter.Filter( request );

			  // then
			  assertTrue( result.RequestUri.ToString().StartsWith("http://" + xForwardHostAndPort, StringComparison.Ordinal) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetTheBaseUriToTheSameProtocolAsTheXForwardProtoHeader()
		 public virtual void ShouldSetTheBaseUriToTheSameProtocolAsTheXForwardProtoHeader()
		 {
			  // given
			  const string theProtocol = "https";

			  XForwardFilter filter = new XForwardFilter();

			  InBoundHeaders headers = new InBoundHeaders();
			  headers.add( X_FORWARD_PROTO_HEADER_KEY, theProtocol );

			  ContainerRequest request = new ContainerRequest( WEB_APPLICATION, "GET", URI.create( "http://jimwebber.org:1234" ), URI.create( "http://jimwebber.org:1234/foo/bar" ), headers, INPUT_STREAM );

			  // when
			  ContainerRequest result = filter.Filter( request );

			  // then
			  assertThat( result.BaseUri.Scheme, containsString( theProtocol ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetTheRequestUriToTheSameProtocolAsTheXForwardProtoHeader()
		 public virtual void ShouldSetTheRequestUriToTheSameProtocolAsTheXForwardProtoHeader()
		 {
			  // given
			  const string theProtocol = "https";

			  XForwardFilter filter = new XForwardFilter();

			  InBoundHeaders headers = new InBoundHeaders();
			  headers.add( X_FORWARD_PROTO_HEADER_KEY, theProtocol );

			  ContainerRequest request = new ContainerRequest( WEB_APPLICATION, "GET", URI.create( "http://jimwebber.org:1234" ), URI.create( "http://jimwebber.org:1234/foo/bar" ), headers, INPUT_STREAM );

			  // when
			  ContainerRequest result = filter.Filter( request );

			  // then
			  assertThat( result.BaseUri.Scheme, containsString( theProtocol ) );
		 }

		 //Mocking WebApplication leads to flakiness on ibm-jdk, hence
		 //we use a manual mock instead
		 private static readonly WebApplication WEB_APPLICATION = new WebApplicationAnonymousInnerClass();

		 private class WebApplicationAnonymousInnerClass : WebApplication
		 {
			 public override bool Initiated
			 {
				 get
				 {
					  return false;
				 }
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void initiate(com.sun.jersey.api.core.ResourceConfig resourceConfig) throws IllegalArgumentException, com.sun.jersey.api.container.ContainerException
			 public override void initiate( ResourceConfig resourceConfig )
			 {

			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void initiate(com.sun.jersey.api.core.ResourceConfig resourceConfig, com.sun.jersey.core.spi.component.ioc.IoCComponentProviderFactory ioCComponentProviderFactory) throws IllegalArgumentException, com.sun.jersey.api.container.ContainerException
			 public override void initiate( ResourceConfig resourceConfig, IoCComponentProviderFactory ioCComponentProviderFactory )
			 {

			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("CloneDoesntCallSuperClone") @Override public com.sun.jersey.spi.container.WebApplication clone()
			 public override WebApplication clone()
			 {
				  return null;
			 }

			 public override FeaturesAndProperties FeaturesAndProperties
			 {
				 get
				 {
					  return null;
				 }
			 }

			 public override Providers Providers
			 {
				 get
				 {
					  return null;
				 }
			 }

			 public override ResourceContext ResourceContext
			 {
				 get
				 {
					  return null;
				 }
			 }

			 public override MessageBodyWorkers MessageBodyWorkers
			 {
				 get
				 {
					  return null;
				 }
			 }

			 public override ExceptionMapperContext ExceptionMapperContext
			 {
				 get
				 {
					  return null;
				 }
			 }

			 public override HttpContext ThreadLocalHttpContext
			 {
				 get
				 {
					  return null;
				 }
			 }

			 public override ServerInjectableProviderFactory ServerInjectableProviderFactory
			 {
				 get
				 {
					  return null;
				 }
			 }

			 public override RequestListener RequestListener
			 {
				 get
				 {
					  return null;
				 }
			 }

			 public override DispatchingListener DispatchingListener
			 {
				 get
				 {
					  return null;
				 }
			 }

			 public override ResponseListener ResponseListener
			 {
				 get
				 {
					  return null;
				 }
			 }

			 public override void handleRequest( ContainerRequest containerRequest, ContainerResponseWriter containerResponseWriter )
			 {

			 }

			 public override void handleRequest( ContainerRequest containerRequest, ContainerResponse containerResponse )
			 {

			 }

			 public override void destroy()
			 {

			 }

			 public override bool TracingEnabled
			 {
				 get
				 {
					  return false;
				 }
			 }

			 public override void trace( string s )
			 {

			 }
		 }

		 //Using mockito to mock arguments to ContainerRequest leads to flakiness
		 //on ibm jdk, hence the manual mocks
		 private static readonly Stream INPUT_STREAM = new InputStreamAnonymousInnerClass();

		 private class InputStreamAnonymousInnerClass : Stream
		 {
			 public override int read()
			 {
				  return 0;
			 }
		 }
	}

}