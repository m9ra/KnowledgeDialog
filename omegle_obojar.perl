#!/usr/bin/perl

use warnings;
use strict;
use WWW::Mechanize;
use JSON;
use Data::Dumper;
use URI::Escape;

$| = 1;

my $question = shift;
die "usage: $0 \"question\"" if ! defined $question;

my $mech = WWW::Mechanize->new();

while (1) {
  my $url = "http://front7.omegle.com/start?rcs=1&firstevents=1&spid=&ask=".uri_escape($question)."&cansavequestion=1";
  $mech->get( $url );
  sleep 1;
  my $text = $mech->text();
  my $data  = decode_json $text;
  # print Dumper($data);
  my $id = $data->{'clientID'};
  if ($id !~ /^cent/) {
    print "Unexpected id: $id\n";
    next;
  }
  while (1) {
    # print $i, "\n";
    my $response = $mech->post("http://front7.omegle.com/events",
      [ 'id'=>$id ]); # , @ns_headers);
    sleep 1;
  
    # print Dumper($response);
    my $content = $response->content();
    print timestamp(), ": ", $content, "\n";
    last if $content eq "null";
  }
  print "-" x 60;
  print "\n";
}

sub timestamp {

  my ($sec,$min,$hour,$mday,$mon,$year,$wday,$yday,$isdst) = localtime(time);
  $year += 1900;
  $mon += 1;
  return sprintf("%04d%02d%02d-%02d:%02d:%02d", $year, $mon, $mday, $hour, $min, $sec);
}
